using Calculatrice.Application;
using Calculatrice.Domaine.Policies;
using Calculatrice.Domaine.Services;
using Calculatrice.Infrastructure.Evaluation;
using Calculatrice.Infrastructure.Policies;
using Calculatrice.Infrastructure.Strategies;

public static class Program
{
    public static void Main()
    {
        var equation = new Equation();
        var operateurs = new IStrategieOperation[]
        {
            new Addition(),
            new Soustraction(),
            new Multiplication(),
            new Division(),
            new Puissance()
        };

        var policies = new IPolicy[] { new DivisionParZeroPolicy() };

        var evaluateur = new CalculerEquation(equation, operateurs, policies);
        var calculer = new CalculerExpression(evaluateur);

        Console.WriteLine(" -> Calculatrice EZO Test By Jeanpy Mukuna\n");
        Console.WriteLine("Entrez votre calcul et appuyez sur Entrée.");
        Console.WriteLine("Tapez 'exit' ou 'q' pour quitter.\n");

        while (true)
        {
            Console.Write("> ");
            var entree = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(entree))
            {
                Console.WriteLine("Expression vide.\n");
                continue;
            }

            if (entree.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                entree.Equals("q", StringComparison.OrdinalIgnoreCase))
                break;

            try
            {
                Console.WriteLine($"= {calculer.Executer(entree)}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}\n");
            }
        }
    }
}
