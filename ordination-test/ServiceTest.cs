namespace ordination_test;

using Microsoft.EntityFrameworkCore;

using Service;
using Data;
using shared.Model;
using static shared.Util;


[TestClass]
public class ServiceTest
{
    private DataService service;

    [TestInitialize]
    public void SetupBeforeEachTest()
    {
        var optionsBuilder = new DbContextOptionsBuilder<OrdinationContext>();
        optionsBuilder.UseInMemoryDatabase(databaseName: "test-database");
        var context = new OrdinationContext(optionsBuilder.Options);
        service = new DataService(context);
        service.SeedData();
    }

    [TestMethod]
    public void PatientsExist()
    {
        Assert.IsNotNull(service.GetPatienter());
    }

    [TestMethod]
    public void OpretDagligFast()
    {
        Patient patient = service.GetPatienter().First();
        Laegemiddel lm = service.GetLaegemidler().First();

        Assert.AreEqual(1, service.GetDagligFaste().Count());

        service.OpretDagligFast(patient.PatientId, lm.LaegemiddelId,
            2, 2, 1, 0, DateTime.Now, DateTime.Now.AddDays(3));

        Assert.AreEqual(2, service.GetDagligFaste().Count());
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TestAtKodenSmiderEnException()
    {
        // Herunder skal man så kalde noget kode,
        // der smider en exception.

        // Hvis koden _ikke_ smider en exception,
        // så fejler testen.

        // Arrange
        service.SeedData();

        // Act - her kaldes med null, som skal kaste ArgumentNullException
        service.AnvendOrdination(1, null!);

        // Assert er ikke nødvendig, da [ExpectedException] fanger resultatet
    }
    
    
//__________________________________________ andre testmetoder jeg har tilføjet:
    [TestMethod]
    public void TestOpretPN()
    {
            // Arrange
            service.SeedData(); // Fylder patienter og lægemidler ind

            var start = new DateTime(2025, 4, 1); //  start- og slutdato for ordinationen
            var slut = new DateTime(2025, 4, 5);
            double antal = 2; // dosen pr. gang =  2 enheder hver gang medicinen gives

            int patientId = 1; // antager at id = 1 findes efter SeedData()
            int laegemiddelId = 1; // antager at id = 1 findes efter SeedData()

            // Act
            var ordination = service.OpretPN(patientId, laegemiddelId, antal, start, slut);

            // Assert
            Assert.IsNotNull(ordination);
            Assert.AreEqual(antal, ordination.antalEnheder);
            Assert.AreEqual(start, ordination.startDen);
            Assert.AreEqual(slut, ordination.slutDen);
        }
    
    
    
    [TestMethod] //Man kan registrere en dosis på en gyldig dato
    public void TestAnvendOrdination()
    {
        // Arrange
        service.SeedData(); // Fylder patienter og ordinationer ind

        // Vi antager at der findes en PN-ordination med ID = 1
        int ordinationId = 1;
        var dato = new shared.Model.Dato(); // tomt objekt 
        dato.dato = new DateTime(2025, 4, 2); // sæt datoen manuelt
        
        // Act
        string resultat = service.AnvendOrdination(ordinationId, dato); // Kaldes med gyldig ordination og dato

        // Assert
        Assert.IsTrue(resultat.Contains("Dosis givet den"));
    }
    
    
    [TestMethod] // beregner en anbefalet daglig dosis baseret på patientens vægt og lægemidlets faktorer.
    public void TestAnbefaletDosisPerDøgn()
    {
        // Arrange
        service.SeedData(); // vi kalder SeedData() for at fylde databasen med testdata
        int patientId = 1;
        int laegemiddelId = 1;

        // Act
        double dosis = service.GetAnbefaletDosisPerDøgn(patientId, laegemiddelId);

        // Assert
        Assert.IsTrue(dosis > 0); // tjekker om resultatet større end 0
    }
    
    
    [TestMethod] // tjekker,om man kan registrere en dosis som givet, hvis datoen er indenfor ordinationens periode.
    public void TestPN_GivDosis_GyldigDato()
    {
        // Arrange
        // Opretter et lægemiddel
        Laegemiddel lm = new Laegemiddel("TestMedicin", 1.0, 1.5, 2.0, "ml");

        // Definerer perioden for ordinationen: 1. april til 5. april
        DateTime start = new DateTime(2025, 4, 1);
        DateTime slut = new DateTime(2025, 4, 5);

        // Opretter PN-ordination med 2 enheder pr. gang
        PN pn = new PN(start, slut, 2, lm);

        // Opretter en dato (2. april) som ligger indenfor perioden
        Dato dato = new Dato();
        dato.dato = new DateTime(2025, 4, 2);

        // Act - vi udfører handlingen: giver dosis på 2. april
        bool resultat = pn.givDosis(dato);

        // - vi tjekker resultatet

        // Bekræfter at metoden returnerede true (dosis blev givet)
        Assert.IsTrue(resultat);

        // Bekræfter at der nu er registreret én dosis i PN-ordinationen
        Assert.AreEqual(1, pn.getAntalGangeGivet());
    }
    //__________________________ (kan måske sletes senere!)
    // Denne test sikrer, at DataService ikke tillader, at man registrerer en dosis udenfor ordinationens gyldighedsperiode.
    // Hvis datoen er udenfor, så skal systemet returnere en fejlbesked ("udenfor perioden").
    [TestMethod]
    public void TestAnvendOrdination_UgyldigDato()
    {
        // Arrange – Forberedelse af testdata

        // Fylder databasen med patienter, lægemidler og ordinationer
        service.SeedData();

        // Antager at der findes en ordination med ID = 1 fra SeedData
        int ordinationId = 1;

        // Opretter en dato som er udenfor gyldighedsperioden (10. april 2025)
        Dato dato = new Dato();
        dato.dato = new DateTime(2025, 4, 10);

        // Act – Udfører handlingen vi vil teste

        // Forsøger at anvende ordinationen på en ugyldig dato
        string resultat = service.AnvendOrdination(ordinationId, dato);

        // Assert – Kontrollerer resultatet

        // Forventer at resultatet indeholder ordet "udenfor", som tegn på fejl
        Assert.IsTrue(resultat.Contains("udenfor"));
    }
    
 // ________________________  kontrollerer, at man kan oprette en DagligSkæv-ordination korrekt gennem DataService.
 // Den tjekker:
 // At en ordination faktisk blev oprettet (IsNotNull)
 // At start- og slutdato er korrekte
 // At antallet af doser er rigtigt (3 doser)

 [TestMethod]
 public void TestOpretDagligSkaev()
 {
     // Arrange – fyld databasen med patienter og lægemidler
     service.SeedData();
     Patient patient       = service.GetPatienter().First();
     Laegemiddel laegemiddel = service.GetLaegemidler().First();
     DateTime startDato    = new DateTime(2025, 4, 1);
     DateTime slutDato     = new DateTime(2025, 4, 5);

     // Opret array af doser, som DataService forventer
     Dosis[] doser = new Dosis[]
     {
         new Dosis(CreateTimeOnly( 8,  0,  0), 1.0),  // Morgen: 1 enhed
         new Dosis(CreateTimeOnly(12,  0,  0), 0.5),  // Middag: 0.5 enhed
         new Dosis(CreateTimeOnly(18,  0,  0), 1.5)   // Aften: 1.5 enhed
     };

     // Act – kald præcis den metode du har i DataService
     var ordination = service.OpretDagligSkaev(
         patient.PatientId,
         laegemiddel.LaegemiddelId,
         doser,
         startDato,
         slutDato
     );

     // Assert – kontroller, at alt blev oprettet korrekt
     Assert.IsNotNull(ordination);                         // Der skal være et objekt
     Assert.AreEqual(startDato,    ordination.startDen);  // Startdato skal være korrekt
     Assert.AreEqual(slutDato,     ordination.slutDen);   // Slutdato skal være korrekt
     Assert.AreEqual(3,            ordination.doser.Count);// Præcis 3 doser
 }

    }



    
