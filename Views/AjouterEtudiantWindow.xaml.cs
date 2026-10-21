using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace GestiCoop.Views
{
    public partial class AjouterEtudiantWindow : Window
    {
        private readonly AjouterEtudiantVM viewModel;

        public Etudiant Resultat => viewModel.Resultat;

        public AjouterEtudiantWindow() : this(null)
        {
        }

        public AjouterEtudiantWindow(Etudiant? existant)
        {
            InitializeComponent();

            viewModel = new AjouterEtudiantVM(existant);
            DataContext = viewModel;

            viewModel.EnregistrementDemande += () =>
            {
                DialogResult = true;
            };
        }
    }

    public class AjouterEtudiantVM : ViewModelBase
    {
        private string nom = "";
        private string prenom = "";
        private string courriel = "";
        private string? programme;
        private string numeroEtudiant = "";

        public string Titre { get; }

        public string[] Programmes { get; } =
        {
            "Informatique",
            "Génie logiciel",
            "Cybersécurité"
        };

        public string Nom
        {
            get => nom;
            set
            {
                nom = value ?? "";
                Notifier();
                ActualiserValidation();
            }
        }

        public string Prenom
        {
            get => prenom;
            set
            {
                prenom = value ?? "";
                Notifier();
                ActualiserValidation();
            }
        }

        public string Courriel
        {
            get => courriel;
            set
            {
                courriel = value ?? "";
                Notifier();
                ActualiserValidation();
            }
        }

        public string? Programme
        {
            get => programme;
            set
            {
                programme = value;
                Notifier();
                ActualiserValidation();
            }
        }

        public string NumeroEtudiant
        {
            get => numeroEtudiant;
            set
            {
                numeroEtudiant = value ?? "";
                Notifier();
                ActualiserValidation();
            }
        }

        public string ErreurNom =>
            string.IsNullOrWhiteSpace(Nom)
                ? "Le nom est obligatoire."
                : "";

        public string ErreurPrenom =>
            string.IsNullOrWhiteSpace(Prenom)
                ? "Le prénom est obligatoire."
                : "";

        public string ErreurCourriel
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Courriel))
                    return "Le courriel est obligatoire.";

                bool valide = Regex.IsMatch(
                    Courriel.Trim(),
                    @"^[^@\s]+@[^@\s.]+(?:\.[^@\s.]+)+$");

                return valide
                    ? ""
                    : "Adresse courriel invalide — ex. : prenom.nom@etud.lacite.ca";
            }
        }

        public bool CourrielInvalide => ErreurCourriel != "";

        public string ErreurProgramme =>
            Programme != null && Programmes.Contains(Programme)
                ? ""
                : "Sélectionne un programme.";

        public string ErreurNumero =>
            NumeroEtudiant.Length == 0 ||
            Regex.IsMatch(NumeroEtudiant, @"^[0-9]{7}$")
                ? ""
                : "Le numéro doit contenir exactement 7 chiffres.";

        public bool EstValide =>
            ErreurNom == "" &&
            ErreurPrenom == "" &&
            ErreurCourriel == "" &&
            ErreurProgramme == "" &&
            ErreurNumero == "";

        public RelayCommand EnregistrerCommand { get; }

        public Etudiant Resultat { get; private set; } = new Etudiant();

        public event Action? EnregistrementDemande;

        public AjouterEtudiantVM(Etudiant? existant = null)
        {
            Titre = existant == null
                ? "Ajouter un étudiant"
                : "Modifier un étudiant";

            if (existant != null)
            {
                nom = existant.Nom;
                prenom = existant.Prenom;
                courriel = existant.Courriel;
                programme = existant.Programme;
                numeroEtudiant = existant.NumeroEtudiant;
            }

            EnregistrerCommand = new RelayCommand(
                _ => Enregistrer(),
                _ => EstValide);
        }

        private void ActualiserValidation()
        {
            Notifier(nameof(ErreurNom));
            Notifier(nameof(ErreurPrenom));
            Notifier(nameof(ErreurCourriel));
            Notifier(nameof(CourrielInvalide));
            Notifier(nameof(ErreurProgramme));
            Notifier(nameof(ErreurNumero));
            Notifier(nameof(EstValide));

            EnregistrerCommand.Actualiser();
        }

        private void Enregistrer()
        {
            if (!EstValide)
                return;

            Resultat = new Etudiant
            {
                Nom = Nom.Trim(),
                Prenom = Prenom.Trim(),
                Courriel = Courriel.Trim(),
                Programme = Programme ?? "",
                NumeroEtudiant = NumeroEtudiant
            };

            EnregistrementDemande?.Invoke();
        }
    }
}