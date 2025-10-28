using MusicCatalog.Models.Korisnici;
using System.Collections.Generic;

namespace MusicCatalog.Repositories
{
    /// <summary>
    /// Definiše operacije za rad sa podacima o korisnicima.
    /// </summary>
    public interface IKorisnikRepository
    {
        /// <summary>
        /// Vraća listu svih korisnika.
        /// </summary>
        List<Korisnik> GetAll();

        /// <summary>
        /// Pronalazi korisnika na osnovu email adrese.
        /// </summary>
        Korisnik? GetByEmail(string email);

        /// <summary>
        /// Dodaje novog korisnika.
        /// </summary>
        void Add(Korisnik korisnik);

        /// <summary>
        /// Ažurira postojećeg korisnika.
        /// </summary>
        void Update(Korisnik korisnik);

        /// <summary>
        /// Briše korisnika na osnovu email adrese.
        /// </summary>
        void Delete(string email);

        /// <summary>
        /// Čuva sve promene u perzistentni storage (npr. JSON fajl).
        /// </summary>
        void SaveChanges();
    }
}