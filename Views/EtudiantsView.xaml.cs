using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace GestiCoop.Views
{
    public partial class EtudiantsView : UserControl
    {
        public EtudiantsView()
        {
            InitializeComponent();
            DataContext = new EtudiantsViewModel();
        }
    }

    public class Etudiant
    {
        public string Nom { get; set; } = "";
        public string Prenom { get; set; } = "";
        public string Programme { get; set; } = "";
        public string Courriel { get; set; } = "";
        public string NumeroEtudiant { get; set; } = "";
    }

    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void Notifier(
            [CallerMemberName] string? propriete = null)
        {
            PropertyChanged?.Invoke(
                this, new PropertyChangedEventArgs(propriete));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object?> action;
        private readonly Predicate<object?>? condition;

        public RelayCommand(
            Action<object?> action,
            Predicate<object?>? condition = null)
        {
            this.action = action;
            this.condition = condition;
        }

        public bool CanExecute(object? parameter)
        {
            return condition?.Invoke(parameter) ?? true;
        }

        public void Execute(object? parameter)
        {
            if (CanExecute(parameter))
                action(parameter);
        }

        public event EventHandler? CanExecuteChanged;

        public void Actualiser()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class EtudiantsViewModel : ViewModelBase
    {
        private readonly ObservableCollection<Etudiant> tousLesEtudiants
            = new ObservableCollection<Etudiant>();

        public ICollectionView Etudiants { get; }

        public string[] Programmes { get; } =
        {
            "Tous les programmes",
            "Informatique",
            "Génie logiciel",
            "Cybersécurité"
        };

        private string recherche = "";

        public string Recherche
        {
            get => recherche;
            set
            {
                recherche = value ?? "";
                Notifier();
                AppliquerFiltre();
            }
        }

        private string programmeFiltre = "Tous les programmes";

        public string ProgrammeFiltre
        {
            get => programmeFiltre;
            set
            {
                programmeFiltre = value ?? "Tous les programmes";
                Notifier();
                AppliquerFiltre();
            }
        }

        public int NombreEtudiants => Etudiants.Cast<Etudiant>().Count();

        public ICommand AjouterCommand { get; }
        public ICommand ModifierCommand { get; }
        public ICommand SupprimerCommand { get; }

        public EtudiantsViewModel()
        {
            tousLesEtudiants.Add(new Etudiant
            {
                Nom = "Tremblay",
                Prenom = "Sophie",
                Programme = "Informatique",
                Courriel = "s.tremblay@etud.lacite.ca"
            });

            tousLesEtudiants.Add(new Etudiant
            {
                Nom = "Nguyen",
                Prenom = "Minh",
                Programme = "Informatique",
                Courriel = "m.nguyen@etud.lacite.ca"
            });

            tousLesEtudiants.Add(new Etudiant
            {
                Nom = "Dubois",
                Prenom = "Élodie",
                Programme = "Génie logiciel",
                Courriel = "e.dubois@etud.lacite.ca"
            });

            tousLesEtudiants.Add(new Etudiant
            {
                Nom = "Okonkwo",
                Prenom = "Chidi",
                Programme = "Cybersécurité",
                Courriel = "c.okonkwo@etud.lacite.ca"
            });

            tousLesEtudiants.Add(new Etudiant
            {
                Nom = "Lavoie",
                Prenom = "Xavier",
                Programme = "Informatique",
                Courriel = "x.lavoie@etud.lacite.ca"
            });

            Etudiants = CollectionViewSource.GetDefaultView(tousLesEtudiants);
            Etudiants.Filter = Filtrer;

            AjouterCommand = new RelayCommand(_ => Ajouter());
            ModifierCommand = new RelayCommand(Modifier);
            SupprimerCommand = new RelayCommand(Supprimer);
        }

        private bool Filtrer(object objet)
        {
            if (objet is not Etudiant etudiant)
                return false;

            string texte = Recherche.Trim();

            bool correspondRecherche =
                texte.Length == 0 ||
                etudiant.Nom.Contains(
                    texte, StringComparison.OrdinalIgnoreCase) ||
                etudiant.Prenom.Contains(
                    texte, StringComparison.OrdinalIgnoreCase) ||
                etudiant.Courriel.Contains(
                    texte, StringComparison.OrdinalIgnoreCase);

            bool correspondProgramme =
                ProgrammeFiltre == "Tous les programmes" ||
                etudiant.Programme == ProgrammeFiltre;

            return correspondRecherche && correspondProgramme;
        }

        public void AppliquerFiltre()
        {
            Etudiants.Refresh();
            Notifier(nameof(NombreEtudiants));
        }

        private void Ajouter()
        {
            var fenetre = new AjouterEtudiantWindow
            {
                Owner = Application.Current.MainWindow
            };

            if (fenetre.ShowDialog() == true)
            {
                tousLesEtudiants.Add(fenetre.Resultat);
                AppliquerFiltre();
            }
        }

        private void Modifier(object? objet)
        {
            if (objet is not Etudiant etudiant)
                return;

            var fenetre = new AjouterEtudiantWindow(etudiant)
            {
                Owner = Application.Current.MainWindow
            };

            if (fenetre.ShowDialog() == true)
            {
                int index = tousLesEtudiants.IndexOf(etudiant);

                if (index >= 0)
                    tousLesEtudiants[index] = fenetre.Resultat;

                AppliquerFiltre();
            }
        }

        private void Supprimer(object? objet)
        {
            if (objet is not Etudiant etudiant)
                return;

            var confirmation = MessageBox.Show(
                $"Supprimer {etudiant.Prenom} {etudiant.Nom} ?",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmation == MessageBoxResult.Yes)
            {
                tousLesEtudiants.Remove(etudiant);
                AppliquerFiltre();
            }
        }
    }
}