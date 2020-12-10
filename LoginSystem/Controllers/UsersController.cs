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

namespace LoginSystem.Controllers
{
    public class UsersController : Controller
    {
        private readonly Context _context;

        public UsersController(Context context)
        {
            _context = context;
        }
        
        [HttpGet]
        public IActionResult RegisterLoginModel()
        {
            return View();
        }

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

        [HttpGet]
        public IActionResult RegisterInformationModel()
        {
            var registerUser = JsonConvert.DeserializeObject<User>(TempData["RegisterUser"].ToString());
            if (registerUser == null) return RedirectToAction(nameof(RegisterLoginModel));
            TempData["RegisterUser"] = JsonConvert.SerializeObject(registerUser);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterInformationModel(RegisterInformationModel registerInformationModel)
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
                _context.Add(registerUser);
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    ViewBag.MsgErro = "Um erro inesperado ocorreu, tente novamente";
                }
            }
            TempData["RegisterUser"] = JsonConvert.SerializeObject(registerUser);
            return View();
        }

        // GET: Users
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

        // GET: Users/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,Name,Email,Password,BirthData,Genre")] User user)
        {
            if (ModelState.IsValid)
            {
                user.PhoneNumber = "987654321";
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
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
            var searchEmail = _context.Users.Where(m => m.Email.ToUpper().Equals(email.ToUpper()));
            if (searchEmail != null) return false;
            return true;
        }
    }
}
