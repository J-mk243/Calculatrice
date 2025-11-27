using Calculatrice.Domaine.Policies;
using Calculatrice.Domaine.Services;

namespace Calculatrice.Infrastructure.Evaluation;

public sealed class CalculerEquation : ICalculerEquation
{
    private readonly Equation _equation;
    private readonly Dictionary<string, IStrategieOperation> _operations;
    private readonly List<IPolicy> _policies;

    public CalculerEquation(Equation equation,
                            IEnumerable<IStrategieOperation> strategies,
                            IEnumerable<IPolicy> policies)
    {
        _equation = equation;
        _operations = strategies.ToDictionary(s => s.Symbole, s => s, StringComparer.Ordinal);
        _policies = policies.ToList();
    }

    public decimal Evaluer(string expression)
    {
        var entrees = _equation.Recuperer(expression).ToList();
        var rpn = ConvertirEnRpn(entrees);
        return EvaluerRpn(rpn);
    }

    private List<Entree> ConvertirEnRpn(List<Entree> entrees)
    {
        var sortieRpn = new List<Entree>();
        var pile = new Stack<Entree>();

        foreach (var t in entrees)
        {
            switch (t.Type)
            {
                case TypeEntree.Nombre:
                    sortieRpn.Add(t);
                    break;

                case TypeEntree.Fonction:
                    pile.Push(t);
                    break;

                case TypeEntree.Operateur:
                    while (pile.Count > 0 && (pile.Peek().Type == TypeEntree.Operateur || pile.Peek().Type == TypeEntree.Fonction))
                    {
                        var top = pile.Peek();

                        if (top.Type == TypeEntree.Fonction)
                        {
                            sortieRpn.Add(pile.Pop());
                            continue;
                        }

                        var opTop = _operations[top.Caractere];
                        var opCur = _operations[t.Caractere];

                        bool condition = opCur.AssocDroite ? opCur.Priorite < opTop.Priorite : opCur.Priorite <= opTop.Priorite;
                        if (condition) sortieRpn.Add(pile.Pop());
                        else break;

                    }
                    pile.Push(t);
                    break;

                case TypeEntree.ParentheseGauche:
                    pile.Push(t);
                    break;

                case TypeEntree.ParentheseDroite:
                    while (pile.Count > 0 && pile.Peek().Type != TypeEntree.ParentheseGauche)
                        sortieRpn.Add(pile.Pop());

                    if (pile.Count == 0)
                        throw new ArgumentException("Parenthèses déséquilibrées.");

                    pile.Pop();

                    if (pile.Count > 0 && pile.Peek().Type == TypeEntree.Fonction)
                        sortieRpn.Add(pile.Pop());
                    break;
            }
        }

        while (pile.Count > 0)
        {
            var op = pile.Pop();
            if (op.Type == TypeEntree.ParentheseGauche || op.Type == TypeEntree.ParentheseDroite)
                throw new ArgumentException("Parenthèses déséquilibrées.");
            sortieRpn.Add(op);
        }
        return sortieRpn;
    }

    private decimal EvaluerRpn(List<Entree> rpn)
    {
        var pile = new Stack<decimal>();

        foreach (var t in rpn)
        {
            if (t.Type == TypeEntree.Nombre)
            {
                pile.Push(t.Valeur!.Value);
                continue;
            }

            if (t.Type == TypeEntree.Fonction)
            {
                if (pile.Count < 1) throw new ArgumentException("Argument manquant pour la fonction.");
                var a = pile.Pop();

                switch (t.Caractere)
                {
                    case "sqrt":
                        if (a < 0) throw new ArgumentException("sqrt sur nombre négatif.");
                        pile.Push((decimal)Math.Sqrt((double)a));
                        break;

                    default:
                        throw new ArgumentException($"Fonction inconnue: {t.Caractere}");
                }
                continue;
            }

            if (pile.Count < 2) throw new ArgumentException("Arguments manquants.");

            var droite = pile.Pop();
            var gauche = pile.Pop();

            foreach (var p in _policies)
                p.Verifier(t.Caractere, gauche, droite);

            if (!_operations.TryGetValue(t.Caractere, out var strat))
                throw new ArgumentException("Opérateur inconnu.");

            pile.Push(strat.Calculer(gauche, droite));
        }

        if (pile.Count != 1) throw new ArgumentException("Expression invalide.");
        return pile.Pop();
    }
}
