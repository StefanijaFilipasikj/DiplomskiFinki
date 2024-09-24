using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiplomskiFinki.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using DiplomskiFinki.Models;
using DiplomskiFinki.Models.Dto;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DiplomskiFinki.Controllers
{
    public class DiplomasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly MailSettings _mailSettings;
        private readonly UserManager<IdentityUser> _userManager;

        public DiplomasController(ApplicationDbContext context, IOptions<MailSettings> mailSettings, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _mailSettings = mailSettings.Value;
            _userManager = userManager;
        }

        // GET: Diplomas
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Diplomas
                .Include(x => x.Student).Include(x => x.Mentor)
                .Include(x => x.Member1).Include(x => x.Member2)
                .Include(x => x.DiplomaStatus).ThenInclude(x => x.Step)
                .ToListAsync());
        }

        // GET: Step
        [Authorize]
        public IActionResult Step()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var diploma = _context.Diplomas
                .Include(x => x.DiplomaStatus)
                .ThenInclude(x => x.Step)
                .FirstOrDefault(x => x.StudentId == Guid.Parse(userId));

            if (diploma != null)
            {
                var step = diploma.DiplomaStatus.Step.SubStep.ToString().Replace(".", "");
                return RedirectToAction($"Step{step}", new { id = diploma.Id });
            }
            else
            {
                return RedirectToAction("Step1");
            }   
        }

        // GET: Diplomas/Step1
        [Authorize]
        //[Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Step1()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUser = _context.Staff.FirstOrDefault(x => x.Id == Guid.Parse(userId));

            var staff = _context.Staff.Select(s => new { s.Id, NameSurname = $"{s.Name} {s.Surname}"}).ToList();
            List<Student> students = _context.Student.ToList();

            ViewBag.Students = new SelectList(students, "Id", "Index");
            ViewBag.Member1Ids = new SelectList(staff, "Id", "NameSurname");
            ViewBag.Member2Ids = new SelectList(staff, "Id", "NameSurname");
            ViewBag.Mentor = currentUser; //mentorot e toj sto e najaven
            return View();
        }


        // POST: Diplomas/Step1Submit
        [HttpPost]
        [Authorize]
        //[Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Step1Submit(Diploma diploma)
        {
            if (ModelState.IsValid)
            {
                var id = Guid.NewGuid();
                diploma.Id = id;
                diploma.Student = _context.Student.FirstOrDefault(x => x.Id == diploma.StudentId);
                diploma.Mentor = _context.Staff.FirstOrDefault(x => x.Id == diploma.MentorId);
                diploma.Member1 = _context.Staff.FirstOrDefault(x => x.Id == diploma.Member1Id);
                diploma.Member2 = _context.Staff.FirstOrDefault(x => x.Id == diploma.Member2Id);
                diploma.ApplicationDate = DateTime.UtcNow;

                var step = _context.Steps.FirstOrDefault(x => x.SubStep == 2);
                var status = new DiplomaStatus
                {
                    Id = Guid.NewGuid(),
                    Step = step,
                    Status = true,
                    Diploma = diploma
                };
                diploma.DiplomaStatus = status;

                _context.Diplomas.Add(diploma);
                _context.DiplomaStatuses.Add(status);
                _context.SaveChanges();

                var link = Url.Action("Step2", "Diplomas", new { id = id }, protocol: Request.Scheme);

                var currentStepName = _context.Steps.FirstOrDefault(x => x.SubStep == 1).SubStepName;
                var body = "<p>По договор, менторот ја пополни формата за нова пријава за дипломски труд.</p>" +
                    "<p>На <a href='" + link + "'>следниот линк</a> можеш да ја потврдиш или одбиеш оваа пријава.</p>";

                SendEmailNotification(diploma.Student.Email, currentStepName, body);

                return RedirectToAction("Index");
            }

            return RedirectToAction("Step1");
        }

        // GET: Diplomas/Step2
        [Authorize]
        //[Authorize(Roles = "Student")]
        public async Task<IActionResult> Step2(Guid id)
        {
            if (checkIfValidStep(id, 2))
            {
                return View(getDiploma(id));
            }
            else
            {
                ViewBag.Error = "You don't have acces to this step";
                return View("DiplomasError");
            }
        }


        // POST: Diplomas/Step2Submit
        [HttpPost]
        [Authorize]
        //[Authorize(Roles = "Student")]
        public async Task<IActionResult> Step2Submit(Guid id, bool accept)
        {
            var diploma = getDiploma(id);
                
            var body = "";
            if (accept == true)
            {
                body = "<p>Студентот со индекс " + diploma.Student.Index  + 
                    " ја прифати пријавата на дипломски труд со id: " + diploma.Id + ".</p>";
            }
            else
            {
                body = "<p>Студентот со индекс " + diploma.Student.Index +
                    " не ја прифати пријавата на дипломски труд со id: " + diploma.Id + ".</p>";
            }

            Step step = updateStep(3, diploma, accept);
            var currentStepName = _context.Steps.FirstOrDefault(x => x.SubStep == 2).SubStepName;

            SendEmailNotification(diploma.Mentor.Email, currentStepName, body);
            // SendEmailNotification("studentski@finki.ukim.mk", currentStepName, body);

            return RedirectToAction("Index");
        }

        // GET: Diplomas/Step3
        [Authorize]
        //[Authorize(Roles = "StudentService")]
        public async Task<IActionResult> Step3(Guid id)
        {
            if (checkIfValidStep(id, 3))
            {
                return View(getDiploma(id));
            }
            else
            {
                ViewBag.Error = "You don't have acces to this step";
                return View("DiplomasError");
            }
        }

        // POST: Diplomas/Step3
        [HttpPost]
        [Authorize]
        //[Authorize(Roles = "StudentService")]
        public async Task<IActionResult> Step3Submit(Guid id, bool accept)
        {
            var diploma = getDiploma(id);
            var credits = diploma.Student.Credits;
            //if (credits >= 240 && diploma.Student.Courses.Any(x => x.Name == "Дипломска Работа") //racno pregleduva sluzba ili?
            
            Step step = updateStep(3.1, diploma, accept);
            var currentStepName = _context.Steps.FirstOrDefault(x => x.SubStep == 3).SubStepName;
            var body = "";

            if (accept)
            {
                body = "<p>Дипломската на студентот со индекс " + diploma.Student.Index + " ги исполнува условите за пријава на тема.</p>" +
                    "<p>Истата е потврдена од студентска служба</p>";
            }
            else
            {
                body = "<p>Во моментот го немаш потрeбниот број на кредити (" + credits + "/240 кредити)." +
                    "или не ти е пријавен предметот Дипломска Работа.</p>";
            }
            SendEmailNotification(diploma.Student.Email, currentStepName, body);
            SendEmailNotification(diploma.Mentor.Email, currentStepName, body);
            SendEmailNotification(diploma.Member1.Email, currentStepName, body);
            SendEmailNotification(diploma.Member2.Email, currentStepName, body);
            return RedirectToAction("Index");
        }

        // GET: Diplomas/Step31
        [Authorize]
        //[Authorize(Roles = "ViceDean")] //prodekan
        public async Task<IActionResult> Step31(Guid id)
        {
            if (checkIfValidStep(id, 3.1))
            {
                return View(getDiploma(id));
            }
            else
            {
                ViewBag.Error = "You don't have acces to this step";
                return View("DiplomasError");
            }
        }

        // POST: Diplomas/Step31
        [HttpPost]
        [Authorize]
        //[Authorize(Roles = "ViceDean")] //prodekan
        public async Task<IActionResult> Step31Submit(Guid id, bool accept)
        {
            var diploma = getDiploma(id);
            Step step = updateStep(4, diploma, accept);

            if (accept)
            {
                var currentStepName = _context.Steps.FirstOrDefault(x => x.SubStep == 3.1).SubStepName;

                var body = "<p>Дипломската на студентот со индекс " + diploma.Student.Index + " соодветствува со областа и менторот.</p>" +
                    "<p>Истата е потврдена од продеканот на настава</p>";

                SendEmailNotification(diploma.Student.Email, currentStepName, body);
                SendEmailNotification(diploma.Mentor.Email, currentStepName, body);
                SendEmailNotification(diploma.Member1.Email, currentStepName, body);
                SendEmailNotification(diploma.Member2.Email, currentStepName, body);
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Step31", new { id = id }); //TODO: show some type of message
            }
        }

        // GET: Diplomas/Step4
        [Authorize]
        //[Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Step4(Guid id)
        {
            if (checkIfValidStep(id, 4))
            {
                return View(getDiploma(id));
            }
            else
            {
                ViewBag.Error = "You don't have acces to this step";
                return View("DiplomasError");
            }
        }

        // POST: Diplomas/Step4Submit
        [HttpPost]
        [Authorize]
        //[Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Step4Submit(Guid id, IFormFile file)
        {
            var diploma = getDiploma(id);
            var fileName = "diplomska_" + diploma.Student.Index + "_" + diploma.Title + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            diploma.FilePath = "/uploads/" + fileName;

            Step step = updateStep(5, diploma, true);
            var currentStepName = _context.Steps.FirstOrDefault(x => x.SubStep == 4).SubStepName;

            //TODO: add href here
            var body = "<p>Дипломската на студентот со индекс " + diploma.Student.Index + " е прикачена.</p>" +
                "<p>Истата можете да ја погледнете на " + "<a href='#'>следниот линк</a></p>";

            SendEmailNotification(diploma.Student.Email, currentStepName, body);
            SendEmailNotification(diploma.Member1.Email, currentStepName, body);
            SendEmailNotification(diploma.Member2.Email, currentStepName, body);

            return RedirectToAction("Index");
        }

        // GET: Diplomas/Step5
        [Authorize]
        //[Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Step5(Guid id)
        {
            if (checkIfValidStep(id, 5))
            {
                return View(getDiploma(id));
            }
            else
            {
                ViewBag.Error = "You don't have acces to this step";
                return View("DiplomasError");
            }
        }

        // POST: Diplomas/Step5Submit
        [HttpPost]
        [Authorize]
        //[Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Step5Submit(Guid id, bool accept, string? note)
        {
            //TODO: check 3 days limit

            var diploma = getDiploma(id);
            Step step = updateStep(6, diploma, accept);
            var currentStepName = _context.Steps.FirstOrDefault(x => x.SubStep == 5).SubStepName;

            //TODO: save note somewhere maybe??
            var accepted = accept == true ? "потврдена" : "одбиена";
            var noted = note != null ? " со забелешка: " + note : "";
            var body = "<p>Дипломската со id: " + diploma.Id + " на студентот со индекс " + diploma.Student.Index +
                " беше " + accepted + noted + ".</p>";

            SendEmailNotification(diploma.Student.Email, currentStepName, body);
            SendEmailNotification(diploma.Mentor.Email, currentStepName, body);
            //SendEmailNotification(studentski@finki.ukim.mk, currentStepName, body);

            return RedirectToAction("Index");
        }

        // GET: Diplomas/Step6
        [Authorize]
        //[Authorize(Roles = "StudentService")]
        public async Task<IActionResult> Step6(Guid id)
        {
            if (checkIfValidStep(id, 6))
            {
                return View(getDiploma(id));
            }
            else
            {
                ViewBag.Error = "You don't have acces to this step";
                return View("DiplomasError");
            }
        }

        // POST: Diplomas/Step6
        [HttpPost]
        [Authorize]
        //[Authorize(Roles = "StudentService")]
        public async Task<IActionResult> Step6Submit(Guid id, bool accept, string? MissingDocuments)
        {
            var diploma = getDiploma(id);
            var step = updateStep(7, diploma, accept);
            var currentStepName = _context.Steps.FirstOrDefault(x => x.SubStep == 6).SubStepName;

            if (accept)
            {
                var body = "<p>Во прилог се наоѓаат потребните документи за одбрана на дипломска</p>";
                //    "<p>" + documents + "</p>";

                SendEmailNotification(diploma.Student.Email, currentStepName, body);
                SendEmailNotification(diploma.Mentor.Email, currentStepName, body);
                SendEmailNotification(diploma.Member1.Email, currentStepName, body);
                SendEmailNotification(diploma.Member2.Email, currentStepName, body);
            }
            else
            {
                var body = "<p>Во прилог се наоѓаат документите кои недостасуваат за одбрана на дипломска</p>" +
                    "<p>" + MissingDocuments + "</p>";
                SendEmailNotification(diploma.Student.Email, currentStepName, body);
            }
            return RedirectToAction("Index");
        }


        // GET: Diplomas/Step7
        [Authorize]
        //[Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Step7(Guid id)
        {
            if (checkIfValidStep(id, 7))
            {
                return View(getDiploma(id));
            }
            else
            {
                ViewBag.Error = "You don't have acces to this step";
                return View("DiplomasError");
            }
        }

        // POST: Step 7
        [HttpPost]
        [Authorize]
        //[Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Step7Submit(Guid id, string roomScheduled, DateTime dateScheduled)
        {
            var diploma = getDiploma(id);
            
            diploma.PresentationDate = dateScheduled;
            diploma.Classroom = roomScheduled;
            _context.Diplomas.Update(diploma);
            _context.SaveChanges();

            var body = "<p>Одбраната на дипломскиот труд на студентот со индекс " + diploma.Student.Index + 
                " ќе се одржи во просторијата " + roomScheduled + ", на  " + dateScheduled + " </p>" +
                "<p>Во прилог се испратени потребните документи.</p>";
            //var body += documents; //TODO

            Step step = updateStep(8, diploma, true);
            var currentStepName = _context.Steps.FirstOrDefault(x => x.SubStep == 7).SubStepName;

            SendEmailNotification(diploma.Student.Email, currentStepName, body);
            SendEmailNotification(diploma.Mentor.Email, currentStepName, body);
            SendEmailNotification(diploma.Member1.Email, currentStepName, body);
            SendEmailNotification(diploma.Member2.Email, currentStepName, body);

            return RedirectToAction("Index");
        }

        // GET: Diplomas/Step8
        [Authorize]
        //[Authorize(Roles = "ViceDean")] //prodekan
        public async Task<IActionResult> Step8(Guid id)
        {
            if (checkIfValidStep(id, 8))
            {
                return View(getDiploma(id));
            }
            else
            {
                ViewBag.Error = "You don't have acces to this step";
                return View("DiplomasError");
            }
        }

        // POST: Step 7
        [HttpPost]
        [Authorize]
        //[Authorize(Roles = "ViceDean")] //prodekan
        public async Task<IActionResult> Step8Submit(Guid id, bool accept)
        {
            var diploma = getDiploma(id);
            Step step = updateStep(8, diploma, accept);
            var currentStepName = _context.Steps.FirstOrDefault(x => x.SubStep == 8).SubStepName;

            var body = "";
            if (accept)
            {
                body = "<p>Дипломскиот труд на студентот со индекс " + diploma.Student.Index +  "е валидирана од продеканот за настава.</p>";
            }
            else
            {
                body = "<p>Дипломскиот труд на студентот со индекс " + diploma.Student.Index + " е одбиена од продеканот за настава.</p>";
            }
            SendEmailNotification(diploma.Student.Email, currentStepName, body);
            SendEmailNotification(diploma.Mentor.Email, currentStepName, body);
            return RedirectToAction("Index");
        }

        private bool checkIfValidStep(Guid id, double step)
        {
            var diploma = getDiploma(id);
            var diplomaStep = diploma.DiplomaStatus.Step.SubStep;
            return diplomaStep == step;
        }

        private Diploma getDiploma(Guid id)
        {
            return _context.Diplomas
                .Include(x => x.Student).Include(x => x.Mentor)
                .Include(x => x.Member1).Include(x => x.Member2)
                .Include(x => x.DiplomaStatus).ThenInclude(x => x.Step)
                .FirstOrDefault(x => x.Id == id);
        }

        public Step updateStep(double stepNum, Diploma diploma, bool diplomaStatus)
        {
            var step = _context.Steps.FirstOrDefault(x => x.SubStep == stepNum);
            var status = _context.DiplomaStatuses.FirstOrDefault(x => x.Diploma.Equals(diploma));
            status.Status = diplomaStatus;
            status.Step = step;

            _context.Diplomas.Update(diploma);
            _context.DiplomaStatuses.Update(status);
            _context.SaveChanges();

            return step;
        }

        public async Task SendEmailNotification(string mail, string subject, string body)
        {
            var emailMessage = new MimeMessage
            {
                Sender = new MailboxAddress("Diplomski Finki", "stefanija.filipasikj@outlook.com"), //TODO: change from mail
                Subject = subject
            };

            emailMessage.From.Add(new MailboxAddress("Diplomski Finki", "stefanija.filipasikj@outlook.com")); //TODO: change from mail
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = body };
            emailMessage.To.Add(new MailboxAddress(mail, mail));

            try
            {
                using (var smtp = new MailKit.Net.Smtp.SmtpClient())
                {
                    var socketOptions = SecureSocketOptions.Auto;
                    await smtp.ConnectAsync(_mailSettings.SmtpServer, 587, socketOptions);

                    if (!string.IsNullOrEmpty(_mailSettings.SmtpUserName))
                    {
                        await smtp.AuthenticateAsync(_mailSettings.SmtpUserName, _mailSettings.SmtpPassword);
                    }
                    await smtp.SendAsync(emailMessage);

                    await smtp.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email: {ex.Message}");
            }
        }
    }
}
