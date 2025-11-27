using Calculatrice.Application;
using Calculatrice.Domaine.Policies;
using Calculatrice.Domaine.Services;
using Calculatrice.Infrastructure.Evaluation;
using Calculatrice.Infrastructure.Policies;
using Calculatrice.Infrastructure.Strategies;

public static class Program
{
    public static void Main(string[] args)
    {
        Equation equation = new Equation();

        IStrategieOperation[] operateurs =
        {
            new Addition(),
            new Soustraction(),
            new Multiplication(),
            new Division(),
            new Puissance()
        };

        DivisionParZeroPolicy divisionPolicy = new DivisionParZeroPolicy();
        IPolicy[] policies = { divisionPolicy };

        ICalculerEquation evaluateur = new CalculerEquation(equation, operateurs, policies);
        CalculerExpression calculer = new CalculerExpression(evaluateur);

        Console.WriteLine(" -> Calculatrice EZO Test By Jeanpy Mukuna\n");
        Console.WriteLine("Entrez votre calcul et appuyez sur Entrée pour voir les résultats.");
        Console.WriteLine("Tapez 'exit' pour quitter.\n");

        while (true)
        {
            Console.Write("> ");
            string? entree = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(entree))
            {
                Console.WriteLine("Expression vide, veuillez entrer un calcul.\n");
                continue;
            }

            if (entree.Equals("Q", StringComparison.OrdinalIgnoreCase))
                break;

            try
            {
                decimal resultat = calculer.Executer(entree);
                Console.WriteLine($"= {resultat}\n");
            }
            catch (DivideByZeroException ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}\n");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur inattendue: {ex.Message}\n");
            }
        }
    }
}
