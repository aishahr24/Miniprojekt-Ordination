namespace shared.Model;

public class DagligSkæv : Ordination {
    public List<Dosis> doser { get; set; } = new List<Dosis>(); // at patienten får medicin hver dag i forskellige tidspunkter

    public DagligSkæv(DateTime startDen, DateTime slutDen, Laegemiddel laegemiddel) : base(laegemiddel, startDen, slutDen) {
	}

    public DagligSkæv(DateTime startDen, DateTime slutDen, Laegemiddel laegemiddel, Dosis[] doser) : base(laegemiddel, startDen, slutDen) {
        this.doser = doser.ToList();
    }    

    public DagligSkæv() : base(null!, new DateTime(), new DateTime()) {
    }

	public void opretDosis(DateTime tid, double antal) {
        doser.Add(new Dosis(tid, antal));
    }

	public override double samletDosis() {
		return base.antalDage() * doegnDosis(); // Hvis antalDage() ikke tæller starten med,
                                          // så bliver den samlede dosis for lille,
                                          // og det vil give forkerte tests og beregninger.
	}

	public override double doegnDosis() {
		// TODO: Implement!
		// returnere den samlede mængde medicin per dag.
		return doser.Sum(d => d.antal); // Gå igennem hele listen (doser) og læg alle antal værdier sammen.
        
	}

	public override String getType() {
		return "DagligSkæv";
	}
}
