#nullable enable

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Mail;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

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
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object?> action;

        public RelayCommand(Action<object?> action)
        {
            this.action = action;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            action(parameter);
        }

        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }
    }

    public class EtudiantsViewModel : INotifyPropertyChanged
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
            get { return recherche; }
            set
            {
                recherche = value ?? "";
                Notifier(nameof(Recherche));
                AppliquerFiltre();
            }
        }

        private string programmeFiltre = "Tous les programmes";

        public string ProgrammeFiltre
        {
            get { return programmeFiltre; }
            set
            {
                programmeFiltre = value ?? "Tous les programmes";
                Notifier(nameof(ProgrammeFiltre));
                AppliquerFiltre();
            }
        }

        public int NombreEtudiants
        {
            get { return Etudiants.Cast<Etudiant>().Count(); }
        }

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

            Etudiants = CollectionViewSource.GetDefaultView(
                tousLesEtudiants);

            Etudiants.Filter = Filtrer;

            AjouterCommand = new RelayCommand(_ => Ajouter());
            ModifierCommand = new RelayCommand(Modifier);
            SupprimerCommand = new RelayCommand(Supprimer);
        }

        private bool Filtrer(object objet)
        {
            Etudiant? etudiant = objet as Etudiant;

            if (etudiant == null)
                return false;

            string texte = Recherche.Trim();

            bool correspondRecherche =
                texte.Length == 0 ||
                etudiant.Nom.IndexOf(
                    texte, StringComparison.OrdinalIgnoreCase) >= 0 ||
                etudiant.Prenom.IndexOf(
                    texte, StringComparison.OrdinalIgnoreCase) >= 0 ||
                etudiant.Courriel.IndexOf(
                    texte, StringComparison.OrdinalIgnoreCase) >= 0;

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
            AjouterEtudiantWindow fenetre =
                new AjouterEtudiantWindow();

            fenetre.Owner = Application.Current.MainWindow;

            if (fenetre.ShowDialog() == true)
            {
                tousLesEtudiants.Add(fenetre.Resultat);
                AppliquerFiltre();
            }
        }

        private void Modifier(object? objet)
        {
            Etudiant? etudiant = objet as Etudiant;

            if (etudiant == null)
                return;

            AjouterEtudiantWindow fenetre =
                new AjouterEtudiantWindow(etudiant);

            fenetre.Owner = Application.Current.MainWindow;

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
            Etudiant? etudiant = objet as Etudiant;

            if (etudiant == null)
                return;

            MessageBoxResult confirmation = MessageBox.Show(
                "Supprimer " + etudiant.Prenom + " " +
                etudiant.Nom + " ?",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmation == MessageBoxResult.Yes)
            {
                tousLesEtudiants.Remove(etudiant);
                AppliquerFiltre();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void Notifier(string propriete)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propriete));
        }
    }

    public class AjouterEtudiantWindow : Window
    {
        private readonly TextBox nom = new TextBox();
        private readonly TextBox prenom = new TextBox();
        private readonly TextBox courriel = new TextBox();
        private readonly ComboBox programme = new ComboBox();

        public Etudiant Resultat { get; private set; }
            = new Etudiant();

        public AjouterEtudiantWindow(Etudiant? existant = null)
        {
            Title = existant == null
                ? "Ajouter un étudiant"
                : "Modifier un étudiant";

            Width = 440;
            SizeToContent = SizeToContent.Height;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = Brushes.White;

            StackPanel panneau = new StackPanel
            {
                Margin = new Thickness(25)
            };

            Content = panneau;

            panneau.Children.Add(new TextBlock
            {
                Text = Title,
                FontSize = 22,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 18)
            });

            AjouterChamp(panneau, "Nom", nom);
            AjouterChamp(panneau, "Prénom", prenom);

            programme.ItemsSource = new[]
            {
                "Informatique",
                "Génie logiciel",
                "Cybersécurité"
            };

            programme.SelectedIndex = 0;

            AjouterChamp(panneau, "Programme", programme);
            AjouterChamp(panneau, "Courriel", courriel);

            StackPanel boutons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 15, 0, 0)
            };

            Button annuler = new Button
            {
                Content = "Annuler",
                Padding = new Thickness(15, 8, 15, 8),
                Margin = new Thickness(0, 0, 10, 0),
                IsCancel = true
            };

            Button enregistrer = new Button
            {
                Content = "Enregistrer",
                Padding = new Thickness(15, 8, 15, 8),
                Background = new SolidColorBrush(
                    Color.FromRgb(41, 59, 119)),
                Foreground = Brushes.White,
                IsDefault = true
            };

            annuler.Click += (sender, e) =>
            {
                DialogResult = false;
            };

            enregistrer.Click += (sender, e) =>
            {
                Enregistrer();
            };

            boutons.Children.Add(annuler);
            boutons.Children.Add(enregistrer);
            panneau.Children.Add(boutons);

            if (existant != null)
            {
                nom.Text = existant.Nom;
                prenom.Text = existant.Prenom;
                courriel.Text = existant.Courriel;
                programme.SelectedItem = existant.Programme;
            }
        }

        private static void AjouterChamp(
            StackPanel panneau,
            string titre,
            Control champ)
        {
            panneau.Children.Add(new TextBlock
            {
                Text = titre,
                Margin = new Thickness(0, 0, 0, 4)
            });

            champ.Height = 34;
            champ.Margin = new Thickness(0, 0, 0, 10);
            champ.Padding = new Thickness(6);

            panneau.Children.Add(champ);
        }

        private void Enregistrer()
        {
            if (string.IsNullOrWhiteSpace(nom.Text) ||
                string.IsNullOrWhiteSpace(prenom.Text) ||
                string.IsNullOrWhiteSpace(courriel.Text) ||
                programme.SelectedItem == null)
            {
                MessageBox.Show(
                    this,
                    "Remplis tous les champs.",
                    "Validation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            string adresse = courriel.Text.Trim();

            try
            {
                MailAddress adresseValide = new MailAddress(adresse);

                if (adresseValide.Address != adresse)
                    throw new FormatException();
            }
            catch (FormatException)
            {
                MessageBox.Show(
                    this,
                    "Entre une adresse courriel valide.",
                    "Validation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            Resultat = new Etudiant
            {
                Nom = nom.Text.Trim(),
                Prenom = prenom.Text.Trim(),
                Programme = programme.SelectedItem.ToString() ?? "",
                Courriel = adresse
            };

            DialogResult = true;
        }
    }
}