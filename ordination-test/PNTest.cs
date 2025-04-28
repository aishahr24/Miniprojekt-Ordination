using Microsoft.VisualStudio.TestTools.UnitTesting;
using shared.Model;
using System;
// sikrer, at man ikke kan registrere en dosis på en forkert dato (udenfor ordinationens periode) for en PN-ordination.
namespace ordination_test
{
    [TestClass]
    public class PNTest
    {
        [TestMethod]
        public void TestPN_GivDosis_UgyldigDato()
        {
            // Arrange – Forberedelse af testdata

            // Opretter et lægemiddel (med vilkårlige faktorer, fordi de ikke bruges her)
            Laegemiddel lm = new Laegemiddel("TestMedicin", 1.0, 1.5, 2.0, "ml");

            // Definerer start- og slutdato for ordinationen: 1. april til 5. april 2025
            DateTime start = new DateTime(2025, 4, 1);
            DateTime slut = new DateTime(2025, 4, 5);

            // Opretter en PN-ordination (2 enheder pr. gang)
            PN pn = new PN(start, slut, 2, lm);

            // Opretter en dato (10. april 2025) som er UDENFOR ordinationens gyldighedsperiode
            Dato dato = new Dato();
            dato.dato = new DateTime(2025, 4, 10);

            // Act – Udfører handlingen vi vil teste

            // Forsøger at give en dosis på en ugyldig dato
            bool resultat = pn.givDosis(dato);

            // Assert – Kontrollerer resultatet

            // Forventer at resultatet er false, fordi datoen er udenfor gyldig periode
            Assert.IsFalse(resultat);

            // Forventer at der ikke er registreret nogen dosis (antal givninger = 0)
            Assert.AreEqual(0, pn.getAntalGangeGivet());
        }
    }
}