using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MAPatients.Models;
using Microsoft.AspNetCore.Http;

namespace MAPatients.Controllers
{
    public class MAMedicationController : Controller
    {
        private readonly PatientsContext _context;

        public MAMedicationController(PatientsContext context)
        {
            _context = context;
        }

        // GET: MAMedication
        public async Task<IActionResult> Index(string id,string name)
        {
            if (String.IsNullOrWhiteSpace(id))
            {
                if (String.IsNullOrEmpty(HttpContext.Session.GetString("code")))
                {
                    TempData["Error"] = "No medication type code available";

                    return RedirectToAction("Index", "MAMedicationType");

                }
                else
                {
                    id = HttpContext.Session.GetString("code");
                }
            }
            else
            {
                HttpContext.Session.SetString("code", id);
            }
            if (String.IsNullOrWhiteSpace(name) || String.IsNullOrEmpty(name))
            {
                name = _context.MedicationType.FirstOrDefault(c => c.MedicationTypeId == Convert.ToInt32(id)).Name;
            }
            ViewData["Name"] = name;
            ViewData["medicationName"] = name;
            var patientsContext = _context.Medication
                .Include(m => m.ConcentrationCodeNavigation)
                .Include(m => m.DispensingCodeNavigation).Include(m => m.MedicationType)
                .Where(m => m.MedicationTypeId.ToString() == id) 
               .OrderBy(m=>m.Name).ThenBy(m=>m.Concentration) ;
            return View(await patientsContext.ToListAsync());
        }

        // GET: MAMedication/Details/5
        public async Task<IActionResult> Details(string id,string medicationName)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medication = await _context.Medication
                .Include(m => m.ConcentrationCodeNavigation)
                .Include(m => m.DispensingCodeNavigation)
                .Include(m => m.MedicationType)
                .FirstOrDefaultAsync(m => m.Din == id);
            if (medication == null)
            {
                return NotFound();
            }
            ViewData["medicationName"] = medicationName;
            return View(medication);
        }

        // GET: MAMedication/Create
        public IActionResult Create(string medicationName)
        {
            ViewData["ConcentrationCode"] = new SelectList(_context.ConcentrationUnit.OrderBy(m => m.ConcentrationCode), "ConcentrationCode", "ConcentrationCode");
            ViewData["DispensingCode"] = new SelectList(_context.DispensingUnit.OrderBy(m=>m.DispensingCode), "DispensingCode", "DispensingCode");
            ViewData["MedicationTypeId"] = new SelectList(_context.MedicationType, "MedicationTypeId", "Name");
            ViewData["medicationName"] = medicationName;
            return View();
        }

        // POST: MAMedication/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string medicationName,[Bind("Din,Name,Image,MedicationTypeId,DispensingCode,Concentration,ConcentrationCode")] Medication medication)
        {
            if (_context.Medication.Any(m => m.Name == medication.Name) && _context.Medication.Any(m => m.Concentration == medication.Concentration) && _context.Medication.Any(m => m.ConcentrationCode == medication.ConcentrationCode))
            {
                ModelState.AddModelError("Name", "Medication already exists");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(medication);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    return View("Error", new ErrorViewModel { ErrorMessage = " An error occured while saving the new entries.please, fill the empty fields" });
                }
            }
            ViewData["ConcentrationCode"] = new SelectList(_context.ConcentrationUnit.OrderBy(m => m.ConcentrationCode), "ConcentrationCode", "ConcentrationCode", medication.ConcentrationCode);
            ViewData["DispensingCode"] = new SelectList(_context.DispensingUnit.OrderBy(m => m.DispensingCode), "DispensingCode", "DispensingCode", medication.DispensingCode);
            ViewData["MedicationTypeId"] = new SelectList(_context.MedicationType, "MedicationTypeId", "Name", medication.MedicationTypeId);
            ViewData["medicationName"] = medicationName;

            return View(medication);
        }

        // GET: MAMedication/Edit/5
        public async Task<IActionResult> Edit(string id,string medicationName)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medication = await _context.Medication.FindAsync(id);
            if (medication == null)
            {
                return NotFound();
            }
            ViewData["ConcentrationCode"] = new SelectList(_context.ConcentrationUnit, "ConcentrationCode", "ConcentrationCode", medication.ConcentrationCode);
            ViewData["DispensingCode"] = new SelectList(_context.DispensingUnit, "DispensingCode", "DispensingCode", medication.DispensingCode);
            ViewData["MedicationTypeId"] = new SelectList(_context.MedicationType, "MedicationTypeId", "Name", medication.MedicationTypeId);
            ViewData["medicationName"] = medicationName;
            return View(medication);
        }

        // POST: MAMedication/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id,string medicationName, [Bind("Din,Name,Image,MedicationTypeId,DispensingCode,Concentration,ConcentrationCode")] Medication medication)
        {
            if (id != medication.Din)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(medication);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MedicationExists(medication.Din))
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
            ViewData["ConcentrationCode"] = new SelectList(_context.ConcentrationUnit, "ConcentrationCode", "ConcentrationCode", medication.ConcentrationCode);
            ViewData["DispensingCode"] = new SelectList(_context.DispensingUnit, "DispensingCode", "DispensingCode", medication.DispensingCode);
            ViewData["MedicationTypeId"] = new SelectList(_context.MedicationType, "MedicationTypeId", "Name", medication.MedicationTypeId);
            ViewData["medicationName"] = medicationName;
            return View(medication);
        }

        // GET: MAMedication/Delete/5
        public async Task<IActionResult> Delete(string id,string medicationName)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medication = await _context.Medication
                .Include(m => m.ConcentrationCodeNavigation)
                .Include(m => m.DispensingCodeNavigation)
                .Include(m => m.MedicationType)
                .FirstOrDefaultAsync(m => m.Din == id);
            if (medication == null)
            {
                return NotFound();
            }
            ViewData["medicationName"] = medicationName;

            return View(medication);
        }

        // POST: MAMedication/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                var medication = await _context.Medication.FindAsync(id);
                _context.Medication.Remove(medication);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            catch (Exception)
            {
                return View("Error", new ErrorViewModel { ErrorMessage = " in order to delete the medication, you have to delete related any MedicationType using this Type" });
            }
        }
private bool MedicationExists(string id)
        {
            return _context.Medication.Any(e => e.Din == id);
        }
    }
}
