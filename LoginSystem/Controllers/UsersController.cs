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
            if (!String.IsNullOrEmpty(HttpContext.Session.GetString("UserId"))) return RedirectToAction(nameof(UserProfile)); 
            return View();
        }

        /// <summary>
        /// [GET] Tela de cadastro parte 2 do usuário
        /// </summary>
        [HttpGet]
        public IActionResult RegisterInformationModel()
        {
            if (!String.IsNullOrEmpty(HttpContext.Session.GetString("UserId"))) return RedirectToAction(nameof(UserProfile));
            if (TempData["RegisterUser"] == null) return RedirectToAction(nameof(UserLogin));

            var registerUser = JsonConvert.DeserializeObject<User>(TempData["RegisterUser"].ToString());
            if (registerUser == null) return RedirectToAction(nameof(RegisterLoginModel));
            TempData["RegisterUser"] = JsonConvert.SerializeObject(registerUser);

            return View();
        }

        /// <summary>
        /// [GET] Tela de aviso de envio de email 
        /// </summary>
        [HttpGet]
        public IActionResult EmailConfirm()
        {
            if (!String.IsNullOrEmpty(HttpContext.Session.GetString("UserId"))) return RedirectToAction(nameof(UserProfile));
            if (TempData["RegisterUser"] == null) return RedirectToAction(nameof(UserLogin));

            ViewBag.Msg = TempData["Erro"];
            ViewBag.Email = TempData["EmailUser"];
            TempData["RegisterUser"] = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<User>(TempData["RegisterUser"].ToString()));
            return View();
        }

        /// <summary>
        /// [GET] Tela de finalizar cadastro
        /// </summary>
        [HttpGet]
        public IActionResult EndUserRegister(string id)
        {
            if (!String.IsNullOrEmpty(HttpContext.Session.GetString("UserId"))) return RedirectToAction(nameof(UserProfile));

            if (TempData["token"] != null && id.Equals(TempData["token"].ToString()))
            {
                User registerUser = JsonConvert.DeserializeObject<User>(TempData["RegisterUser"].ToString());
                if (registerUser == null) return RedirectToAction(nameof(RegisterLoginModel));

                return View(registerUser);
            }
            return RedirectToAction(nameof(UserLogin));
        }

        /// <summary>
        /// [GET] Tela de login de usuário
        /// </summary>
        [HttpGet]
        public IActionResult UserLogin()
        {
            if (!String.IsNullOrEmpty(HttpContext.Session.GetString("UserId"))) return RedirectToAction(nameof(UserProfile));
            return View();
        }

        /// <summary>
        /// [GET] Tela de dados do usuário logado
        /// </summary>
        [HttpGet]
        public IActionResult UserProfile()
        {
            if (String.IsNullOrEmpty(HttpContext.Session.GetString("UserId"))) return RedirectToAction(nameof(UserLogin));
            int id = Int32.Parse(HttpContext.Session.GetString("UserId"));
            User user = _context.Users.Find(id);
            return View(user);
        }
        
        /// <summary>
        /// [GET] Editar usuário
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> UserEdit(int? id)
        {
            if (String.IsNullOrEmpty(HttpContext.Session.GetString("UserId"))) return RedirectToAction(nameof(UserLogin));
            if (id == null)
            {
                return NotFound();
            }
            
            //Se tentar usar id de outro usuario redireciona pro usuario logado
            if(id.ToString() != HttpContext.Session.GetString("UserId").ToString()) 
            {
                int? idFind = Int32.Parse(HttpContext.Session.GetString("UserId").ToString());
                return View(idFind);
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        /// <summary>
        /// [POST] Editar usuário
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserEdit(int id, User user)
        {
            if (id != user.UserId)
            {
                return NotFound();
            }
            if (user.BirthData >= DateTime.Now) ModelState.AddModelError("BirthData", "A data de nascimento é inválida");
            ViewBag.msg = null;
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(UserProfile));
                }
                catch (DbUpdateConcurrencyException)
                {
                    ViewBag.msg = "Um erro inesperado ocorreu, tente novamente";
                    if (!UserExists(user.UserId))
                    {
                        return NotFound();
                    }
                }
            }
            return View(user);
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
        /// [POST] Tela de finalizar cadastro
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EndUserRegister(User user)
        {
            ViewBag.Msg = null; 
            try
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                TempData["token"] = null;
                StartSessionLogin(user);
                return RedirectToAction(nameof(UserProfile));
            }
            catch
            {
                ViewBag.Msg = "Um erro ocorreu, tente novamente";
                return View();
            }
        }

        /// <summary>
        /// [POST] Tela de login de usuário
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UserLogin(LoginModel loginModel) 
        {
            ViewBag.Erro = null;
            if (ModelState.IsValid)
            {
                if (Login(loginModel.Email, loginModel.Password))
                {
                    return RedirectToAction(nameof(UserProfile));
                }
                ViewBag.Erro = "E-mail ou senha incorretos";
            }
            return View();
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

        /// <summary>
        /// Verifica se um email já esta cadastrado
        /// </summary>
        /// <param name="email">email a ser verificado</param>
        private bool UserEmailExists(string email)
        {
            //Verifica se o email já foi cadastrado
            User searchEmail = _context.Users.Where(m => m.Email.ToUpper().Equals(email.ToUpper())).FirstOrDefault();
            if (searchEmail != null) return true;
            return false;
        }

        /// <summary>
        /// Envia email pro usuário
        /// </summary>
        /// <param name="email">E-mail pro remetente</param>
        /// <param name="title">Título do e-mail</param>
        /// <param name="msg">Mensagem do e-mail</param>
        /// <param name="link">Link de recuperação do e-mail</param>
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

        /// <summary>
        /// Efetua o login do usuário
        /// </summary>
        public bool Login(string email, string password)
        {
            Cryptography cryptography = new Cryptography(MD5.Create());
            string passwordCript = cryptography.HashGenerate(password);

            User user = _context.Users.Where(p => p.Email.ToUpper().Equals(email.ToUpper()) && p.Password.Equals(passwordCript)).FirstOrDefault();
            if (user == null) return false;

            StartSessionLogin(user);
            return true;
        }

        /// <summary>
        /// Deslogado o usuário - Remove as sessions existentes
        /// </summary>
        public void Logout()
        {
            HttpContext.Session.Remove("UserName");
            HttpContext.Session.Remove("UserEmail");
            HttpContext.Session.Remove("UserId");
        }

        /// <summary>
        /// Inicia a session
        /// </summary>
        private void StartSessionLogin(User user)
        {
            HttpContext.Session.SetString("UserName", user.Name);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserId", user.UserId.ToString());
        }
    }
}
