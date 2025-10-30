using MusicCatalog.Models;
using MusicCatalog.Models;
using MusicCatalog.Models;
using MusicCatalog.Models;
using MusicCatalog.Models.MuzickiSadrzaj;
using MusicCatalog.Models.Umetnici;
using MusicCatalog.Models.Recenzije;
using MusicCatalog.Repositories;
using MusicCatalog.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace MusicCatalog.ViewModels
{
    public class UmetnikInfoViewModel : ViewModelBase
    {
        
        private readonly IMuzickiUmetnikRepository _umetnikRepo;
        private readonly IClanstvoRepository _clanstvoRepo;
        private readonly IMuzickoDeloRepository _deloRepo;
        private readonly IRecenzijaRepository _recenzijaRepo;
        private readonly IOcenaRepository _ocenaRepo;
        public MuzickiUmetnik Umetnik { get; }
        public bool IsBend { get; }
        public string Title { get; }

        public ObservableCollection<MuzickoDelo> Albumi { get; } = new();
        public ObservableCollection<MuzickoDelo> Pesme { get; } = new();
        public ObservableCollection<Izvodjac> Clanovi { get; } = new();
        public ObservableCollection<Recenzija> Recenzije { get; } = new();
        public ObservableCollection<Ocena> Ocene { get; } = new();

        public ICommand CloseCommand { get; }
        public Action? CloseWindow { get; set; }

        public UmetnikInfoViewModel(
            MuzickiUmetnik umetnik, 
            IMuzickiUmetnikRepository umetnikRepo, 
            IClanstvoRepository clanstvoRepo, 
            IMuzickoDeloRepository deloRepo,
            IRecenzijaRepository recenzijaRepo,
            IOcenaRepository ocenaRepo)
        {
            Umetnik = umetnik;
            _umetnikRepo = umetnikRepo;
            _clanstvoRepo = clanstvoRepo;
            _deloRepo = deloRepo;
            _recenzijaRepo = recenzijaRepo;
            _ocenaRepo = ocenaRepo;
            IsBend = umetnik is Bend;
            Title = IsBend ? "Informacije o Bendu" : "Informacije o Izvođaču";

            CloseCommand = new RelayCommand(_ => CloseWindow?.Invoke());

            LoadDetails();
        }

        private void LoadDetails()
        {
            
            if (IsBend)
            {
                var clanstva = _clanstvoRepo.GetAll().Where(c => c.BendId == Umetnik.Id);
                foreach (var clanstvo in clanstva)
                {
                    var izvodjac = _umetnikRepo.GetById(clanstvo.UmetnikId) as Izvodjac;
                    if (izvodjac != null)
                    {
                        Clanovi.Add(izvodjac);
                    }
                }
            }

            
            if (Umetnik.MuzickoDeloIDs != null)
            {
                var idSet = Umetnik.MuzickoDeloIDs.ToHashSet();
                var svaDela = _deloRepo.GetAll().Where(d => idSet.Contains(d.Id));

                foreach (var delo in svaDela)
                {
                    if (delo is Album album)
                    {
                        Albumi.Add(album);
                    }
                    else if (delo is Pesma pesma)
                    {
                        Pesme.Add(pesma);
                    }
                }
            }

            var recenzije = _recenzijaRepo.GetAll().Where(r => r.Id == Umetnik.Id);
            foreach (var recenzija in recenzije)
            {
                Recenzije.Add(recenzija);
            }

            var ocene = _ocenaRepo.GetAll().Where(o => o.Id == Umetnik.Id);
            foreach (var ocena in ocene)
            {
                Ocene.Add(ocena);
            }
        }
    }
}