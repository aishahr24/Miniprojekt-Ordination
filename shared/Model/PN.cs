namespace shared.Model;

// PN = "pro necesse" = efter behov
public class PN : Ordination { 
	public double antalEnheder { get; set; }
    public List<Dato> dates { get; set; } = new List<Dato>(); // alle dage medicinen blev givet

    public PN (DateTime startDen, DateTime slutDen, double antalEnheder, Laegemiddel laegemiddel) : base(laegemiddel, startDen, slutDen) {
		this.antalEnheder = antalEnheder;
	}

    public PN() : base(null!, new DateTime(), new DateTime()) {
    }

    /// <summary>
    /// Registrerer at der er givet en dosis på dagen givesDen
    /// Returnerer true hvis givesDen er inden for ordinationens gyldighedsperiode og datoen huskes
    /// Returner false ellers og datoen givesDen ignoreres
    /// </summary>
    
 //________________   
    // Tjekke om givesDen ligger indenfor startDen og slutDen
    // Hvis ja → læg datoen i dates, og returnér true
    // Hvis nej → ignorer den og returner false
    public bool givDosis(Dato givesDen) {
        // TODO: Implement!
        if (givesDen.dato >= startDen && givesDen.dato <= slutDen)
        {
	        dates.Add(givesDen);
	        return true;
        }
        return false;
    }

    
    // beregne gennemsnitlig døgndosis, kun i den periode hvor der gives medicin, og kun mellem første og sidste dato
    public override double doegnDosis() {
    	// TODO: Implement!
	    if (dates.Count == 0)
		    return 0; // hvis medicinen er ikke givet (dates er tøm) så retunere 0, --> da vi ikke kan udregne noget

	    var minDato = dates.Min(d => d.dato); // minDato = den første dato medicinen blev givet
	    var maxDato = dates.Max(d => d.dato); // maxDato = den sidste dato medicinen blev givet
	    int dage = (maxDato - minDato).Days + 1; //  beregner antal dage mellem første og sidste dag, inklusiv begge dage

	    return (dates.Count * antalEnheder) / (double)dage; // døgndosis = (antal gange givet * antalEnheder" hvor meget") / antal dage mellem første og sidste givning (inklusive)
    }


    public override double samletDosis() {
        return dates.Count() * antalEnheder;
    }

    public int getAntalGangeGivet() {
        return dates.Count();
    }

	public override String getType() {
		return "PN";
	}
}
