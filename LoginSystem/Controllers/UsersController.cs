using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LoginSystem.Data;
using LoginSystem.Models;
using LoginSystem.ViewModel;
using Newtonsoft.Json;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;

namespace LoginSystem.Controllers
{
    public class UsersController : Controller
    {
        private readonly Context _context;

        /// <summary>
        /// Metódo construtor 
        /// </summary>
        public UsersController(Context context)
        {
            _context = context;
        }
        
        /// <summary>
        /// [GET] Tela de cadastro parte 1 do usuário
        /// </summary>
        [HttpGet]
        public IActionResult RegisterLoginModel()
        {
            return View();
        }

        /// <summary>
        /// [POST] Tela de cadastro parte 1 do usuário
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegisterLoginModel(RegisterLoginModel registerLoginModel)
        {
            var user = new User();
            Cryptography cryptography = new Cryptography(System.Security.Cryptography.MD5.Create());

            //Verifica se email existe
            if(UserEmailExists(registerLoginModel.Email)) ModelState.AddModelError("Email", "O e-mail inserido já existe");

            //Verifica se a senha a confirmação de senha são iguais
            if (!cryptography.HashVerify(registerLoginModel.PasswordConfirm, registerLoginModel.Password))
            {
                ModelState.AddModelError("Password", "A senha e a confirmação de senha não são iguais");
            }
            //Verifica força da senha
            else if (user.VerifyPasswordStrong(registerLoginModel.Password) < 3)
            {
                ModelState.AddModelError("Password", "A segurança da senha é baixa");
            }

            if (ModelState.IsValid)
            {
                user.Name = registerLoginModel.Name.ToUpper();
                user.Email = registerLoginModel.Email.ToUpper();
                user.Password = cryptography.HashGenerate(registerLoginModel.Password);
                TempData["RegisterUser"] = JsonConvert.SerializeObject(user);
                return RedirectToAction(nameof(RegisterInformationModel));
            }

            registerLoginModel.Password = null;
            registerLoginModel.PasswordConfirm = null;
            return View(registerLoginModel);
        }

        /// <summary>
        /// [GET] Tela de cadastro parte 2 do usuário
        /// </summary>
        [HttpGet]
        public IActionResult RegisterInformationModel()
        {
            var registerUser = JsonConvert.DeserializeObject<User>(TempData["RegisterUser"].ToString());
            if (registerUser == null) return RedirectToAction(nameof(RegisterLoginModel));
            TempData["RegisterUser"] = JsonConvert.SerializeObject(registerUser);

            return View();
        }

        /// <summary>
        /// [POST] Tela de cadastro parte 2 do usuário
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegisterInformationModel(RegisterInformationModel registerInformationModel)
        {
            User registerUser = JsonConvert.DeserializeObject<User>(TempData["RegisterUser"].ToString());
            if (registerUser == null) return RedirectToAction(nameof(RegisterLoginModel));
        
            // Verifica se a data de aniversário é inválida
            if (registerInformationModel.BirthData >= DateTime.Now) ModelState.AddModelError("BirthData", "A data de nascimento é inválida");

            ViewBag.MsgErro = null;
            if (ModelState.IsValid)
            {
                registerUser.BirthData = registerInformationModel.BirthData;
                registerUser.Genre = registerInformationModel.Genre;
                registerUser.PhoneNumber = registerInformationModel.PhoneNumber;
                
                //Enviar e-mail pra confirmar email pra cadastro 
                SendEmail(registerUser.Email, "Confirmar o cadastro", "Você criou uma conta no sistema de login .NET CORE, clique no link para confirmar a criação da conta", "https://localhost:44332/Users/EndUserRegister?id=");
                TempData["RegisterUser"] = JsonConvert.SerializeObject(registerUser);
                TempData["EmailUser"] = registerUser.Email;
                return RedirectToAction(nameof(EmailConfirm));
            }
            TempData["RegisterUser"] = JsonConvert.SerializeObject(registerUser);
            return View();
        }
        
        /// <summary>
        /// [GET] Tela de aviso de envio de email 
        /// </summary>
        [HttpGet]
        public IActionResult EmailConfirm() 
        {
            ViewBag.Msg = TempData["Erro"];
            ViewBag.Email = TempData["EmailUser"];
            return View();

        }
        
        /// <summary>
        /// [GET] Tela de finalizar cadastro
        /// </summary>
        [HttpGet]
        public IActionResult EndUserRegister(string id) 
        {
            if (TempData["token"] != null && id != TempData["token"] as string)
            {
                User registerUser = JsonConvert.DeserializeObject<User>(TempData["RegisterUser"].ToString());
                if (registerUser == null) return RedirectToAction(nameof(RegisterLoginModel));

                return View(registerUser);
            }
            return RedirectToAction(nameof(RegisterLoginModel));
        }

        /// <summary>
        /// [POST] Tela de finalizar cadastro
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> EndUserRegister(User user)
        {
            ViewBag.Msg = null; 
            try
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                TempData["token"] = null;
                HttpContext.Session.SetString("UserName", user.Name);
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("UserId", user.UserId.ToString());
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ViewBag.Msg = "Um erro ocorreu, tente novamente";
                return View();
            }
        }

        public bool Login(string email, string password)
        {
            Cryptography cryptography = new Cryptography(MD5.Create());
            string passwordCript = cryptography.HashGenerate(password);

            User user = _context.Users.Where(p => p.Email.ToUpper().Equals(email.ToUpper()) && p.Password.Equals(passwordCript)).FirstOrDefault();
            if (user == null) return false;
            
            HttpContext.Session.SetString("UserName", user.Name);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserId", user.UserId.ToString());
            return true;
        }

        public void Logout() 
        {
            HttpContext.Session.Remove("UserName");
            HttpContext.Session.Remove("UserEmail");
            HttpContext.Session.Remove("UserId");
        }


        [Authorize]
        [HttpGet]
        public IActionResult UserProfile() 
        {
            User user = _context.Users.Find(HttpContext.Session.GetString("UserId"));
            return View(user);
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,Name,Email,Password,BirthData,Genre")] User user)
        {
            if (id != user.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.UserId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }

        private bool UserEmailExists(string email)
        {
            //Verifica se o email já foi cadastrado
            User searchEmail = _context.Users.Where(m => m.Email.ToUpper().Equals(email.ToUpper())).FirstOrDefault();
            if (searchEmail != null) return true;
            return false;
        }

        public void SendEmail(string email, string title, string msg, string link)
        {
            try
            {
                string token = Guid.NewGuid().ToString();

                MailMessage m = new MailMessage(new MailAddress("controlicsenai@gmail.com", title), new MailAddress(email));
                m.Subject = "Confirmação de Email";
                m.Body = string.Format(@"Olá usuário,
                                            <br/> 
                                            {0}
                                            <br/>
                                            <br/> 
                                            <a href=""{1}{2}"" title=User Email Confirm>Link</a>",
                                        msg, link, token);

                TempData["token"] = token;

                m.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential("controlicsenai@gmail.com", "controlic4");
                smtp.EnableSsl = true;
                smtp.Send(m);
            }
            catch
            {
                TempData["Erro"] = "Um erro aconteceu. Tente novamente.";
            }
        }
    }
}
