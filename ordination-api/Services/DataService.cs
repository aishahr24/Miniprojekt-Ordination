using Microsoft.EntityFrameworkCore;
using System.Text.Json;

using shared.Model;
using static shared.Util;
using Data;
 
namespace Service;

public class DataService
{
    private OrdinationContext db { get; }

    public DataService(OrdinationContext db) {
        this.db = db;
    }
    
    public void SeedData() {

        // Patients
        Patient[] patients = new Patient[5];
        patients[0] = db.Patienter.FirstOrDefault()!;

        if (patients[0] == null)
        {
            patients[0] = new Patient("121256-0512", "Jane Jensen", 63.4);
            patients[1] = new Patient("070985-1153", "Finn Madsen", 83.2);
            patients[2] = new Patient("050972-1233", "Hans Jørgensen", 89.4);
            patients[3] = new Patient("011064-1522", "Ulla Nielsen", 59.9);
            patients[4] = new Patient("123456-1234", "Ib Hansen", 87.7);

            db.Patienter.Add(patients[0]);
            db.Patienter.Add(patients[1]);
            db.Patienter.Add(patients[2]);
            db.Patienter.Add(patients[3]);
            db.Patienter.Add(patients[4]);
            db.SaveChanges();
        }

        Laegemiddel[] laegemiddler = new Laegemiddel[5];
        laegemiddler[0] = db.Laegemiddler.FirstOrDefault()!;
        if (laegemiddler[0] == null)
        {
            laegemiddler[0] = new Laegemiddel("Acetylsalicylsyre", 0.1, 0.15, 0.16, "Styk");
            laegemiddler[1] = new Laegemiddel("Paracetamol", 1, 1.5, 2, "Ml");
            laegemiddler[2] = new Laegemiddel("Fucidin", 0.025, 0.025, 0.025, "Styk");
            laegemiddler[3] = new Laegemiddel("Methotrexat", 0.01, 0.015, 0.02, "Styk");
            laegemiddler[4] = new Laegemiddel("Prednisolon", 0.1, 0.15, 0.2, "Styk");

            db.Laegemiddler.Add(laegemiddler[0]);
            db.Laegemiddler.Add(laegemiddler[1]);
            db.Laegemiddler.Add(laegemiddler[2]);
            db.Laegemiddler.Add(laegemiddler[3]);
            db.Laegemiddler.Add(laegemiddler[4]);

            db.SaveChanges();
        }

        Ordination[] ordinationer = new Ordination[6];
        ordinationer[0] = db.Ordinationer.FirstOrDefault()!;
        if (ordinationer[0] == null) {
            Laegemiddel[] lm = db.Laegemiddler.ToArray();
            Patient[] p = db.Patienter.ToArray();

            ordinationer[0] = new PN(new DateTime(2021, 1, 1), new DateTime(2021, 1, 12), 123, lm[1]);    
            ordinationer[1] = new PN(new DateTime(2021, 2, 12), new DateTime(2021, 2, 14), 3, lm[0]);    
            ordinationer[2] = new PN(new DateTime(2021, 1, 20), new DateTime(2021, 1, 25), 5, lm[2]);    
            ordinationer[3] = new PN(new DateTime(2021, 1, 1), new DateTime(2021, 1, 12), 123, lm[1]);
            ordinationer[4] = new DagligFast(new DateTime(2021, 1, 10), new DateTime(2021, 1, 12), lm[1], 2, 0, 1, 0);
            ordinationer[5] = new DagligSkæv(new DateTime(2021, 1, 23), new DateTime(2021, 1, 24), lm[2]);
            
            ((DagligSkæv) ordinationer[5]).doser = new Dosis[] { 
                new Dosis(CreateTimeOnly(12, 0, 0), 0.5),
                new Dosis(CreateTimeOnly(12, 40, 0), 1),
                new Dosis(CreateTimeOnly(16, 0, 0), 2.5),
                new Dosis(CreateTimeOnly(18, 45, 0), 3)        
            }.ToList();
            

            db.Ordinationer.Add(ordinationer[0]);
            db.Ordinationer.Add(ordinationer[1]);
            db.Ordinationer.Add(ordinationer[2]);
            db.Ordinationer.Add(ordinationer[3]);
            db.Ordinationer.Add(ordinationer[4]);
            db.Ordinationer.Add(ordinationer[5]);

            db.SaveChanges();

            p[0].ordinationer.Add(ordinationer[0]);
            p[0].ordinationer.Add(ordinationer[1]);
            p[2].ordinationer.Add(ordinationer[2]);
            p[3].ordinationer.Add(ordinationer[3]);
            p[1].ordinationer.Add(ordinationer[4]);
            p[1].ordinationer.Add(ordinationer[5]);

            db.SaveChanges();
        }
    }

    
    public List<PN> GetPNs() {
        return db.PNs.Include(o => o.laegemiddel).Include(o => o.dates).ToList();
    }

    public List<DagligFast> GetDagligFaste() {
        return db.DagligFaste
            .Include(o => o.laegemiddel)
            .Include(o => o.MorgenDosis)
            .Include(o => o.MiddagDosis)
            .Include(o => o.AftenDosis)            
            .Include(o => o.NatDosis)            
            .ToList();
    }

    public List<DagligSkæv> GetDagligSkæve() {
        return db.DagligSkæve
            .Include(o => o.laegemiddel)
            .Include(o => o.doser)
            .ToList();
    }

    public List<Patient> GetPatienter() {
        return db.Patienter.Include(p => p.ordinationer).ToList();
    }

    public List<Laegemiddel> GetLaegemidler() {
        return db.Laegemiddler.ToList();
    }
//________________________ PN = Pro Necessitate "efter behov"
    public PN OpretPN(int patientId, int laegemiddelId, double antal, DateTime startDato, DateTime slutDato) { // Opretter en ny PN-ordination for en patient
        // Finder patienten i databasen eller kaster fejl hvis ikke fundet
        Patient patient = db.Patienter.Find(patientId) ?? throw new Exception("Patient ikke fundet");

        // Finder lægemidlet i databasen eller kaster fejl hvis ikke fundet
        Laegemiddel laegemiddel = db.Laegemiddler.Find(laegemiddelId) ?? throw new Exception("Lægemiddel ikke fundet");

        // Opreter ny PN-ordination (antal = hvor meget medicin pr. gang)
        var ordination = new PN(startDato, slutDato, antal, laegemiddel);

        // Tilføjer ordinationen til databasen og til patientens liste
        db.Ordinationer.Add(ordination);
        patient.ordinationer.Add(ordination);

        // Gemmer ændringerne i databasen
        db.SaveChanges();

        // Returner ordinationen
        return ordination;
    }
//___________________________ 
    public DagligFast OpretDagligFast(int patientId, int laegemiddelId, 
        double antalMorgen, double antalMiddag, double antalAften, double antalNat, 
        DateTime startDato, DateTime slutDato) {

        Patient patient = db.Patienter.Find(patientId) ?? throw new Exception("Patient ikke fundet"); // Finder patienten i databasen
        Laegemiddel laegemiddel = db.Laegemiddler.Find(laegemiddelId) ?? throw new Exception("Lægemiddel ikke fundet"); // Finder medicinen

        var ordination = new DagligFast(startDato, slutDato, laegemiddel, antalMorgen, antalMiddag, antalAften, antalNat); // Opretter selve ordinationen
        db.Ordinationer.Add(ordination);// Lægger den i databasen

        patient.ordinationer.Add(ordination); // 	Kobler den til patienten

        db.SaveChanges(); 
        return ordination;
    }
//___________________________ Patienten får medicin hver dag, men på forskellige tidspunkter og med forskellige doser
    public DagligSkæv OpretDagligSkaev(int patientId, int laegemiddelId, Dosis[] doser, DateTime startDato, DateTime slutDato) {
        // Finder patienten i databasen baseret på ID eller kaster fejl
        Patient patient = db.Patienter.Find(patientId) ?? throw new Exception("Patient ikke fundet");

        // Finder lægemidlet i databasen baseret på ID eller kaster fejl
        Laegemiddel laegemiddel = db.Laegemiddler.Find(laegemiddelId) ?? throw new Exception("Lægemiddel ikke fundet");

        // Opreter en ny DagligSkæv-ordination med doser (doser konverteres til array)
        var ordination = new DagligSkæv(startDato, slutDato, laegemiddel, doser.ToArray());

        // Tilføj ordinationen til databasen
        db.Ordinationer.Add(ordination);

        // Tilføj ordinationen til patientens ordinationsliste
        patient.ordinationer.Add(ordination);

        
        db.SaveChanges();

        // Returner den nye ordination
        return ordination;
    }
    
//___________________________ registrere en dosis som givet, men kun for PN-ordinationer
    public string AnvendOrdination(int id, Dato dato) { 
        // Tjekker for null – hvis dato er null, kast ArgumentNullException
        if (dato is null)
            throw new ArgumentNullException(nameof(dato));

        // Henter ordinationen fra databasen på baggrund af id
        //    Hvis ikke fundet, kast Exception
        Ordination ordination = db.Ordinationer.Find(id)
                                ?? throw new Exception("Ordination ikke fundet");

        // Tjeker om det er en PN-ordination (efter behov)
        if (ordination is PN pn)
        {
            // Forsøger at registrere dosis på den angivne dato
            bool succes = pn.givDosis(dato);

            if (succes)
            {
                // Hvis registreringen lykkedes, gem i databasen
                db.SaveChanges();
                // 6) Returner bekræftelsesbesked med dato
                return $"Dosis givet den {dato.dato.ToShortDateString()}";
            }
            else
            {
                // Hvis dato er udenfor perioden, returner fejlbesked
                return $"Dato {dato.dato.ToShortDateString()} er udenfor ordinationens periode";
            }
        }
        else
        {
            // Hvis ordinationen ikke er PN-type, returnér relevant besked
            return "Ordinationen er ikke en PN-ordination og kan ikke anvendes manuelt";
        }
    }

    /// <summary>
    /// Den anbefalede dosis for den pågældende patient, per døgn, hvor der skal tages hensyn til
	/// patientens vægt. Enheden afhænger af lægemidlet. Patient og lægemiddel må ikke være null.
    /// </summary>
    /// <param name="patient"></param>
    /// <param name="laegemiddel"></param>
    /// <returns></returns>
    
    
//___________________________ beregne den anbefalede døgndosis for en patient baseret på: (står i Lægemiddel.cs)
    // Let: under 25 kg → brug enhedPrKgPrDoegnLet
    // Normal: 25–120 kg → brug enhedPrKgPrDoegnNormal
    // Tung: over 120 kg → brug enhedPrKgPrDoegnTung
    // formel; vægt * faktor = anbefalet døgndosis
	public double GetAnbefaletDosisPerDøgn(int patientId, int laegemiddelId) {
        // TODO: Implement!
        // Finder patient og lægemiddel i databasen
        Patient patient = db.Patienter.Find(patientId) ?? throw new Exception("Patient ikke fundet");
        Laegemiddel laegemiddel = db.Laegemiddler.Find(laegemiddelId) ?? throw new Exception("Lægemiddel ikke fundet");

        double faktor;

        // Vælger faktor ud fra patientens vægt
        if (patient.vaegt < 25)
        {
            faktor = laegemiddel.enhedPrKgPrDoegnLet;
        }
        else if (patient.vaegt > 120)
        {
            faktor = laegemiddel.enhedPrKgPrDoegnTung;
        }
        else
        {
            faktor = laegemiddel.enhedPrKgPrDoegnNormal;
        }

        // Udregner anbefalet dosis
        double dosis = patient.vaegt * faktor;

        return dosis;
	}
    


}