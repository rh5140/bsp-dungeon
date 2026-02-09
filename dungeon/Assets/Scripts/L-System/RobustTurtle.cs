using UnityEngine;
using System.Collections.Generic;
using System.Text;

public class RobustTurtle : MonoBehaviour
{
    // =================================================
    // Turtle settings
    // =================================================
    [Header("Turtle Settings")]
    public float step = 1f;
    public float angle = 25f;

    // which symbols should draw forward
    public string drawSymbols = "FGXYAB";

    // =================================================
    // Start
    // =================================================
    [Header("Random Seed")]
    [SerializeField] bool _useRandomSeed = true;
    [SerializeField] int _seed = 0;

    // =================================================
    // L-System grammar
    // =================================================
    [Header("Grammar")]
    [TextArea] public List<string> axioms = new() { "F" };

    [System.Serializable]
    public class Rule
    {
        public char symbol;
        [TextArea] public string replacement;
    }

    public List<Rule> rules = new();
    public int iterations = 4;
    public bool pickRandomAxiom = false;
    Dictionary<char, string> ruleMap; 
    // Fern reference: https://en.wikipedia.org/wiki/L-system#Example_7:_fractal_plant

    // =================================================
    // Rendering
    // =================================================
    [Header("Line Settings")]
    public Material lineMaterial;
    public float lineWidth = 0.05f;
    public bool useMeshes = false;
    public GameObject segmentPrefab;
    public float thinMultiplier = 0.2f;
    bool thinnedAlready = false;

    LineRenderer line;
    List<Vector3> points = new();

    // =================================================
    // Gradient
    // =================================================
    [Header("Gradient Settings")]
    public Gradient gradient = new Gradient();
    public int totalGradientSteps = 500;
    int gradientStep = 0;

    // =================================================
    // Start
    // =================================================
    void Start()
    {
        RandomSeed();

        SetupLineRenderer();
        BuildRuleMap();

        string seed = ChooseAxiom();
        string result = Generate(seed);

        Interpret(result);

        if (!useMeshes)
            Draw();
    }

    // =================================================
    // Set up random seed
    // =================================================
    void RandomSeed()
    {
        if (_useRandomSeed)
        {
            _seed = System.Environment.TickCount;
        }
        Random.InitState(_seed);
    }

    // =================================================
    // Build dictionary
    // =================================================
    void BuildRuleMap()
    {
        ruleMap = new Dictionary<char, string>();

        foreach (var r in rules)
            ruleMap[r.symbol] = r.replacement;
    }

    // =================================================
    // Pick axiom
    // =================================================
    string ChooseAxiom()
    {
        if (axioms.Count == 0) return "";

        if (pickRandomAxiom)
            return axioms[Random.Range(0, axioms.Count)];

        return axioms[0];
    }

    // =================================================
    // Rewrite string
    // =================================================
    string Generate(string current)
    {
        for (int i = 0; i < iterations; i++)
        {
            StringBuilder next = new();

            foreach (char c in current)
            {
                if (ruleMap.ContainsKey(c))
                    next.Append(ruleMap[c]);
                else
                    next.Append(c);
            }

            current = next.ToString();
        }

        return current;
    }

    // =================================================
    // Turtle interpreter
    // =================================================
    void Interpret(string commands)
    {
        Stack<Vector3> positions = new Stack<Vector3>();
        Stack<Quaternion> rotations = new Stack<Quaternion>();

        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;

        points.Add(pos);

        foreach (char c in commands)
        {
            // draw symbols (F,G,X,Y,A,B...)
            if (drawSymbols.Contains(c))
            {
                Vector3 newPos = pos + rot * Vector3.up * step;

                if (useMeshes && segmentPrefab != null)
                    SpawnSegment(pos, newPos);

                points.Add(newPos);
                pos = newPos;
            }

            float pitch = RandomizeAngle(-angle, angle); 
            float roll = RandomizeAngle(-angle, angle); 

            switch (c)
            {
                case '+': 
                    rot *= Quaternion.Euler(0, 0, angle); 
                    if (Probability(50))
                    {
                        rot *= Quaternion.Euler(pitch,0,0);
                    }
                    if (Probability(50))
                    {
                        rot *= Quaternion.Euler(0, roll, 0);
                    }
                    break;
                case '-': 
                    rot *= Quaternion.Euler(0, 0, -angle);  
                    if (Probability(50))
                    {
                        rot *= Quaternion.Euler(pitch,0,0);
                    }
                    if (Probability(50))
                    {
                        rot *= Quaternion.Euler(0, roll, 0);
                    }
                    break;
                case '[':
                    if (!thinnedAlready)
                    {
                        lineWidth *= thinMultiplier;
                        thinnedAlready = true;
                    }
                    positions.Push(pos);
                    rotations.Push(rot);
                    break;
                case ']':
                    pos = positions.Pop();
                    rot = rotations.Pop();
                    break;
            }
        }
    }

    float RandomizeAngle(float min, float max)
    {
        return Random.Range(min, max);
    }

    bool Probability(float percentage)
    {
        int rand = Random.Range(0,100);
        if (rand < percentage) return true;
        else return false;
    }

    // =================================================
    // Rendering
    // =================================================
    void SetupLineRenderer()
    {
        line = gameObject.AddComponent<LineRenderer>();
        line.material = lineMaterial;
        line.widthMultiplier = lineWidth;
        line.useWorldSpace = true;
    }

    void Draw()
    {
        line.positionCount = points.Count;
        line.SetPositions(points.ToArray());
    }

    void SpawnSegment(Vector3 a, Vector3 b)
    {
        GameObject seg = Instantiate(segmentPrefab, transform);
        seg.transform.position = (a + b) * 0.5f;
        seg.transform.up = (b - a);
        seg.transform.localScale =
            new Vector3(lineWidth, (b - a).magnitude * 0.5f, lineWidth);
        
        // Temporary color change code
        seg.GetComponent<MeshRenderer>().material.color = GetGradientColor((float)gradientStep/totalGradientSteps);
    }

    Color GetGradientColor(float proportion)
    {
        gradientStep++;
        if (proportion > 1) return gradient.Evaluate(1);
        return gradient.Evaluate(proportion);
    }
}
