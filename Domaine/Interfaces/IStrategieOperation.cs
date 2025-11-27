namespace Calculatrice.Domaine.Services;
public interface IStrategieOperation
{
    string Symbole { get; }
    int Priorite { get; }
    bool AssocDroite { get; }
    decimal Calculer(decimal gauche, decimal droite);
}
