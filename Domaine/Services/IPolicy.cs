namespace Calculatrice.Domaine.Policies;
public interface IPolicy
{
    void Verifier(string operateur, decimal gauche, decimal droite);
}
