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

namespace DiplomskiFinki.Controllers
{
    public class DiplomasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly MailSettings _mailSettings;

        public DiplomasController(ApplicationDbContext context, IOptions<MailSettings> mailSettings)
        {
            _context = context;
            _mailSettings = mailSettings.Value;
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

        // GET: Diplomas/Step1
        public async Task<IActionResult> Step1()
        {
            var staff = _context.Staff.Select(s => new { s.Id, NameSurname = $"{s.Name} {s.Surname}"}).ToList();
            List<Student> students = _context.Student.ToList();

            ViewBag.Students = new SelectList(students, "Id", "Index");
            ViewBag.Member1Ids = new SelectList(staff, "Id", "NameSurname");
            ViewBag.Member2Ids = new SelectList(staff, "Id", "NameSurname");
            return View();
        }


        // POST: Diplomas/Step1Submit
        [HttpPost]
        public async Task<IActionResult> Step1Submit(Diploma diploma)
        {
            if (ModelState.IsValid)
            {
                //TODO: mentorot e toj sto e najaven
                Staff mentor = _context.Staff.ElementAt(0);
                diploma.MentorId = mentor.Id;

                var id = Guid.NewGuid();
                diploma.Id = id;
                diploma.Student = _context.Student.FirstOrDefault(x => x.Id == diploma.StudentId);
                diploma.Mentor = _context.Staff.FirstOrDefault(x => x.Id == diploma.MentorId);
                diploma.Member1 = _context.Staff.FirstOrDefault(x => x.Id == diploma.Member1Id);
                diploma.Member2 = _context.Staff.FirstOrDefault(x => x.Id == diploma.Member2Id);
                diploma.ApplicationDate = DateTime.UtcNow;

                _context.Diplomas.Add(diploma);

                var step = _context.Steps.FirstOrDefault(x => x.SubStep == 1);

                var status = new DiplomaStatus
                {
                    Id = Guid.NewGuid(),
                    Step = step,
                    Status = true,
                    Diploma = diploma
                };

                _context.DiplomaStatuses.Add(status);

                _context.SaveChanges();

                var link = Url.Action("Step2", "Diplomas", new { id = id }, protocol: Request.Scheme);

                var body = "<p>По договор, менторот ја пополни формата за нова пријава за дипломски труд.</p>" +
                    "<p>На <a href='" + link + "'>следниот линк</a> можеш да ја потврдиш или одбиеш оваа пријава.</p>";

                SendEmailNotification(diploma.Student.Email, step.SubStepName, body);

                return RedirectToAction("Index");
            }

            return RedirectToAction("Step1");
        }

        // GET: Diplomas/Step2
        public async Task<IActionResult> Step2(Guid id)
        {
            return View(getDiploma(id));
        }


        // POST: Diplomas/Step2Submit
        [HttpPost]
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

            var step = _context.Steps.FirstOrDefault(x => x.SubStep == 2);
            var status = _context.DiplomaStatuses.FirstOrDefault(x => x.Diploma.Equals(diploma));
            status.Status = accept;
            status.Step = step;

            _context.DiplomaStatuses.Update(status);
            _context.SaveChanges();

            SendEmailNotification(diploma.Mentor.Email, step.SubStepName, body);
            // SendEmailNotification("sluzba@outlook.com", step.SubStepName, body);

            return RedirectToAction("Index");
        }


        // GET: Diplomas/Step3
        public async Task<IActionResult> Step3(Guid id)
        {
            //TODO
            return RedirectToAction("Index");
        }
        //Also TODO: Step 3.1



        // GET: Diplomas/Step4
        public async Task<IActionResult> Step4(Guid id)
        {
            return View(getDiploma(id));
        }

        // POST: Diplomas/Step4Submit
        [HttpPost]
        public async Task<IActionResult> Step4Submit(Guid id, IFormFile file)
        {
            var diploma = getDiploma(id);

            var fileName = Path.GetFileNameWithoutExtension(file.FileName) + "_" + Guid.NewGuid() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            diploma.FilePath = "/uploads/" + fileName;

            var step = _context.Steps.FirstOrDefault(x => x.SubStep == 4);
            var status = _context.DiplomaStatuses.FirstOrDefault(x => x.Diploma.Equals(diploma));
            status.Step = step;

            _context.Diplomas.Update(diploma);
            _context.DiplomaStatuses.Update(status);
            _context.SaveChanges();

            //TODO: add href here
            var body = "<p>Дипломската на студентот со индекс " + diploma.Student.Index + " е прикачена.</p>" +
                "<p>Истата можете да ја погледнете на " + "<a href='#'>следниот линк</a></p>";

            SendEmailNotification(diploma.Student.Email, step.SubStepName, body);
            SendEmailNotification(diploma.Member1.Email, step.SubStepName, body);
            SendEmailNotification(diploma.Member2.Email, step.SubStepName, body);

            return RedirectToAction("Index");
        }

        // GET: Diplomas/Step5
        public async Task<IActionResult> Step5(Guid id)
        {
            return View(getDiploma(id));
        }

        // POST: Diplomas/Step5Submit
        [HttpPost]
        public async Task<IActionResult> Step5Submit(Guid id, bool accept, string? note)
        {
            //TODO: check 3 days limit

            var diploma = getDiploma(id);

            var step = _context.Steps.FirstOrDefault(x => x.SubStep == 5);
            var status = _context.DiplomaStatuses.FirstOrDefault(x => x.Diploma.Equals(diploma));
            status.Step = step;
            status.Status = accept;

            //TODO: save note somewhere maybe??

            _context.DiplomaStatuses.Update(status);
            _context.SaveChanges();

            var accepted = accept == true ? "потврдена" : "одбиена";
            var noted = note != null ? " со забелешка: " + note : "";
            var body = "<p>Дипломската со id: " + diploma.Id + " на студентот со индекс " + diploma.Student.Index +
                " беше " + accepted + noted + ".</p>";

            SendEmailNotification(diploma.Student.Email, step.SubStepName, body);
            SendEmailNotification(diploma.Mentor.Email, step.SubStepName, body);
            //SendEmailNotification(sluzba@outlook.com, step.SubStepName, body);

            return RedirectToAction("Index");
        }


        // GET: Diplomas/Step6
        public async Task<IActionResult> Step6(Guid id)
        {
            var diploma = getDiploma(id);
            
            var step = _context.Steps.FirstOrDefault(x => x.SubStep == 6);

            var credits = diploma.Student.Credits;
            if(credits >= 240)
            {
                var body = "<p>Во прилог се наоѓаат потребните документи за одбрана на дипломска</p>";
                //var body += documents; //TODO

                SendEmailNotification(diploma.Student.Email, step.SubStepName, body);
                SendEmailNotification(diploma.Mentor.Email, step.SubStepName, body);
                SendEmailNotification(diploma.Member1.Email, step.SubStepName, body);
                SendEmailNotification(diploma.Member2.Email, step.SubStepName, body);
            }
            else
            {
                var body = "<p>Во моментот го немаш потрeбниот број на кредити (" + credits + "/240 кредити).</p>";
                SendEmailNotification(diploma.Student.Email, step.SubStepName, body);
            }
            return RedirectToAction("Index");
        }


        // GET: Diplomas/Step7
        public async Task<IActionResult> Step7(Guid id)
        {
            return View(getDiploma(id));
        }

        // POST: Step 7
        [HttpPost]
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

            var step = _context.Steps.FirstOrDefault(x => x.SubStep == 7);

            SendEmailNotification(diploma.Student.Email, step.SubStepName, body);
            SendEmailNotification(diploma.Mentor.Email, step.SubStepName, body);
            SendEmailNotification(diploma.Member1.Email, step.SubStepName, body);
            SendEmailNotification(diploma.Member2.Email, step.SubStepName, body);

            return RedirectToAction("Index");
        }

        // GET: Diplomas/Step8
        public async Task<IActionResult> Step8()
        {
            //TODO
            return RedirectToAction("Index");
        }

        private Diploma getDiploma(Guid id)
        {
            return _context.Diplomas
                .Include(x => x.Student).Include(x => x.Mentor)
                .Include(x => x.Member1).Include(x => x.Member2)
                .Include(x => x.DiplomaStatus).ThenInclude(x => x.Step)
                .FirstOrDefault(x => x.Id == id);
        }

        public async Task SendEmailNotification(string mail, string subject, string body)
        {
            var emailMessage = new MimeMessage
            {
                Sender = new MailboxAddress("Diplomski Finki", "stefanija.filipasikj@outlook.com"),
                Subject = subject
            };

            emailMessage.From.Add(new MailboxAddress("Diplomski Finki", "stefanija.filipasikj@outlook.com"));
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
