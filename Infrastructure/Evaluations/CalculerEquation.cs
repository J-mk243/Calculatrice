using Calculatrice.Domaine.Policies;
using Calculatrice.Domaine.Services;

namespace Calculatrice.Infrastructure.Evaluation;

/// <summary>
/// Service responsable de l'évaluation d'une expression mathématique.
/// Convertit l'expression en RPN (Reverse Polish Notation) puis l'évalue.
/// </summary>
public sealed class CalculerEquation : ICalculerEquation
{
    private readonly Equation _equation;
    private readonly Dictionary<string, IStrategieOperation> _operations;
    private readonly List<IPolicy> _policies;

    public CalculerEquation(
        Equation equation,
        IEnumerable<IStrategieOperation> strategies,
        IEnumerable<IPolicy> policies)
    {
        _equation = equation ?? throw new ArgumentNullException(nameof(equation));
        _operations = strategies?.ToDictionary(s => s.Symbole, s => s, StringComparer.Ordinal)
                      ?? throw new ArgumentNullException(nameof(strategies));
        _policies = policies?.ToList() ?? throw new ArgumentNullException(nameof(policies));
    }

    /// <summary>
    /// Évalue une expression mathématique en chaîne de caractères.
    /// </summary>
    public decimal Evaluer(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("L'expression ne peut pas être vide.", nameof(expression));

        var tabEquation = _equation.ExtraireEntrees(expression).ToList();
        var rpn = ConvertirEnRpn(tabEquation);
        return EvaluerRpn(rpn);
    }

    /// <summary>
    /// Convertit une liste d'entrées en notation polonaise inversée (RPN).
    /// </summary>
    private List<Entree> ConvertirEnRpn(List<Entree> tabEquation)
    {
        var sortieRpn = new List<Entree>();
        var pile = new Stack<Entree>();

        foreach (var caractere in tabEquation)
        {
            switch (caractere.Type)
            {
                case TypeEntree.Nombre:
                    sortieRpn.Add(caractere);
                    break;

                case TypeEntree.Fonction:
                    pile.Push(caractere);
                    break;

                case TypeEntree.Operateur:
                    TraiterOperateur(caractere, pile, sortieRpn);
                    break;

                case TypeEntree.ParentheseGauche:
                    pile.Push(caractere);
                    break;

                case TypeEntree.ParentheseDroite:
                    TraiterParentheseDroite(pile, sortieRpn);
                    break;
            }
        }

        while (pile.Count > 0)
        {
            var op = pile.Pop();
            if (op.Type is TypeEntree.ParentheseGauche or TypeEntree.ParentheseDroite)
                throw new ArgumentException("Parenthèses déséquilibrées.");
            sortieRpn.Add(op);
        }

        return sortieRpn;
    }

    /// <summary>
    /// Évalue une expression en RPN.
    /// </summary>
    private decimal EvaluerRpn(List<Entree> rpn)
    {
        var pile = new Stack<decimal>();

        foreach (var caractere in rpn)
        {
            switch (caractere.Type)
            {
                case TypeEntree.Nombre:
                    pile.Push(caractere.Valeur!.Value);
                    break;

                case TypeEntree.Fonction:
                    EvaluerFonction(caractere, pile);
                    break;

                case TypeEntree.Operateur:
                    EvaluerOperateur(caractere, pile);
                    break;
            }
        }

        if (pile.Count != 1)
            throw new ArgumentException("Expression invalide.");

        return pile.Pop();
    }

    // ----------------------
    // Méthodes privées utilitaires
    // ----------------------

    private void TraiterOperateur(Entree operateur, Stack<Entree> pile, List<Entree> sortieRpn)
    {
        while (pile.Count > 0 && (pile.Peek().Type == TypeEntree.Operateur || pile.Peek().Type == TypeEntree.Fonction))
        {
            var top = pile.Peek();

            if (top.Type == TypeEntree.Fonction)
            {
                sortieRpn.Add(pile.Pop());
                continue;
            }

            var opTop = _operations[top.Chaine];
            var opCur = _operations[operateur.Chaine];

            bool condition = opCur.AssocDroite
                ? opCur.Priorite < opTop.Priorite
                : opCur.Priorite <= opTop.Priorite;

            if (condition) sortieRpn.Add(pile.Pop());
            else break;
        }

        pile.Push(operateur);
    }

    private void TraiterParentheseDroite(Stack<Entree> pile, List<Entree> sortieRpn)
    {
        while (pile.Count > 0 && pile.Peek().Type != TypeEntree.ParentheseGauche)
            sortieRpn.Add(pile.Pop());

        if (pile.Count == 0)
            throw new ArgumentException("Parenthèses déséquilibrées.");

        pile.Pop();

        if (pile.Count > 0 && pile.Peek().Type == TypeEntree.Fonction)
            sortieRpn.Add(pile.Pop());
    }

    private void EvaluerFonction(Entree fonction, Stack<decimal> pile)
    {
        if (pile.Count < 1)
            throw new ArgumentException("Argument manquant pour la fonction.");

        var a = pile.Pop();

        switch (fonction.Chaine)
        {
            case "sqrt":
                if (a < 0) throw new ArgumentException("sqrt sur nombre négatif.");
                pile.Push((decimal)Math.Sqrt((double)a));
                break;

            default:
                throw new ArgumentException($"Fonction inconnue: {fonction.Chaine}");
        }
    }
    private void EvaluerOperateur(Entree operateur, Stack<decimal> pile)
    {
        if (pile.Count < 2)
            throw new ArgumentException("Arguments manquants pour l'opérateur.");

        var droite = pile.Pop();
        var gauche = pile.Pop();

        foreach (var policy in _policies)
            policy.Verifier(operateur.Chaine, gauche, droite);

        if (!_operations.TryGetValue(operateur.Chaine, out var strategie))
            throw new ArgumentException($"Opérateur inconnu: {operateur.Chaine}");

        pile.Push(strategie.Calculer(gauche, droite));
    }
}
