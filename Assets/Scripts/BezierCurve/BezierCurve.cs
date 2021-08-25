using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierCurve : MonoBehaviour
{
    [SerializeField] private Transform _transform0 = default;
    [SerializeField] private Transform _transform1 = default;
    [SerializeField] private Transform _transform2 = default;
    [SerializeField] private Transform _transform3 = default;

    [SerializeField] private float _pointRadius = 0.1f;

    [SerializeField] private int _divide = 10;
    [SerializeField] private float _tolerance = 0.1f;

    [Range(2, 20)]
    [SerializeField] private int _level = 10;
    [SerializeField] private Vector3 _offset = default;

    [SerializeField] private int _pointCount = 0;

    [SerializeField] private int _debugPoint1 = 1;
    [SerializeField] private int _debugPoint2 = 1;

    private Vector3 _startPoint { get => _transform0 ? _transform0.position : Vector3.one; }
    private Vector3 _controlPoint0 { get => _transform1 ? _transform1.position : Vector3.one; }
    private Vector3 _controlPoint1 { get => _transform2 ? _transform2.position : Vector3.one; }
    private Vector3 _endPoint { get => _transform3 ? _transform3.position : Vector3.one; }

    private List<Vector3> _allPoints = new List<Vector3>();
    private List<float> _allT = new List<float>();

    #region Unity_Lifecycle
    //private void Awake() { }
    //private void OnEnable() { }
    //private void Start() { }
    //private void FixedUpdate() { }
    private void Update() { UpdateDebugPoints(); }
    //private void LateUpdate() { }
    private void OnDrawGizmos() { DrawLineGizmos(); }
    //private void OnDisable() { }
    //private void OnDestroy() { }
    #endregion

    public static float GetDistPointToLine(Vector3 origin, Vector3 direction, Vector3 point)
    {
        var pointToOrigin = origin - point;
        var pointToClosestPointOnLine = pointToOrigin - Vector3.Dot(pointToOrigin, direction) * direction;

        return pointToClosestPointOnLine.magnitude;
    }

    private Vector3 Evaluate(float t)
    {
        var it = 1 - t;
        var it2 = it * it;
        var t2 = t * t;

        return it2 * it * _startPoint +
               3f * it2 * t * _controlPoint0 +
               3f * t2 * it * _controlPoint1 +
               t2 * t * _endPoint;
    }

    private Vector3 Evaluate2(float t)
    {
        var evaluate_0_0 = Vector3.Lerp(_startPoint, _controlPoint0, t);
        var evaluate_0_1 = Vector3.Lerp(_controlPoint0, _controlPoint1, t);
        var evaluate_0_2 = Vector3.Lerp(_controlPoint1, _endPoint, t);

        var evaluate_1_0 = Vector3.Lerp(evaluate_0_0, evaluate_0_1, t);
        var evaluate_1_1 = Vector3.Lerp(evaluate_0_1, evaluate_0_2, t);

        return Vector3.Lerp(evaluate_1_0, evaluate_1_1, t);
    }

    private void GeneratePoints(Vector3 a, Vector3 b, float t, int level)
    {
        if (level > _level) { return; }

        var evaluate = Evaluate(t);
        var distance = GetDistPointToLine(a, (b - a).normalized, evaluate);
        if (distance < _tolerance)
        {
            _allPoints.Add(b);
            _allT.Add(t);
            return;
        }

        var step = 1.0f / (1 << level);
        GeneratePoints(a, evaluate, t - step, level + 1);
        GeneratePoints(evaluate, b, t + step, level + 1);
    }

    private void UpdateDebugPoints()
    {
        _debugPoint1 = Mathf.RoundToInt(Mathf.Abs(Mathf.Sin(Time.time * Mathf.PI / 5f) * (_divide - 2))) + 1;
        _debugPoint2 = Mathf.RoundToInt(Mathf.Abs(Mathf.Sin(Time.time * Mathf.PI / 5f) * (_pointCount - 3))) + 1;
    }

    private void DrawLineGizmos()
    {
        Method1();
        Method2();
    }

    private void Method1()
    {
        Gizmos.color = Color.red;

        DrawPoints(_startPoint, _controlPoint0, _controlPoint1, _endPoint);

        var lastPoint = _startPoint;
        for (int i = 1; i <= _divide; i++)
        {
            if (i > _debugPoint1) { return; }

            var t = (float)i / _divide;
            var point = Evaluate(t);

            Gizmos.color = Color.white;
            Gizmos.DrawLine(lastPoint, point);

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(point, _pointRadius * 2f);
            lastPoint = point;

            if (i != _debugPoint1) { continue; }

            var pointGreen0 = Vector3.Lerp(_startPoint, _controlPoint0, t);
            var pointGreen1 = Vector3.Lerp(_controlPoint0, _controlPoint1, t);
            var pointGreen2 = Vector3.Lerp(_controlPoint1, _endPoint, t);

            Debug.DrawLine(pointGreen0, pointGreen1, Color.green);
            Debug.DrawLine(pointGreen1, pointGreen2, Color.green);

            var pointBlue0 = Vector3.Lerp(pointGreen0, pointGreen1, t);
            var pointBlue1 = Vector3.Lerp(pointGreen1, pointGreen2, t);

            Debug.DrawLine(pointBlue0, pointBlue1, Color.blue);
        }
    }

    private void Method2()
    {
        _allPoints.Clear();
        _allT.Clear();
        Gizmos.color = Color.red;

        DrawPoints(_startPoint + _offset, _controlPoint0 + _offset, _controlPoint1 + _offset, _endPoint + _offset);

        GeneratePoints(_startPoint, _endPoint, 0.5f, 2);
        _pointCount = _allPoints.Count;

        var lastPoint = _startPoint + _offset;
        for (int i = 1; i < _allPoints.Count; i++)
        {
            if (i > _debugPoint2) { return; }

            var point = _allPoints[i] + _offset;

            Gizmos.color = Color.white;
            Gizmos.DrawLine(lastPoint, point);

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(point, _pointRadius * 2f);
            lastPoint = point;

            if (i != _debugPoint2) { continue; }

            var pointGreen0 = Vector3.Lerp(_startPoint, _controlPoint0, _allT[i]);
            var pointGreen1 = Vector3.Lerp(_controlPoint0, _controlPoint1, _allT[i]);
            var pointGreen2 = Vector3.Lerp(_controlPoint1, _endPoint, _allT[i]);

            Debug.DrawLine(pointGreen0 + _offset, pointGreen1 + _offset, Color.green);
            Debug.DrawLine(pointGreen1 + _offset, pointGreen2 + _offset, Color.green);

            var pointBlue0 = Vector3.Lerp(pointGreen0, pointGreen1, _allT[i]);
            var pointBlue1 = Vector3.Lerp(pointGreen1, pointGreen2, _allT[i]);

            Debug.DrawLine(pointBlue0 + _offset, pointBlue1 + _offset, Color.blue);
        }
    }

    private void DrawPoints(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Gizmos.DrawSphere(p0, _pointRadius);
        Gizmos.DrawSphere(p1, _pointRadius);
        Gizmos.DrawSphere(p2, _pointRadius);
        Gizmos.DrawSphere(p3, _pointRadius);

        Gizmos.DrawLine(p0, p1);
        Gizmos.DrawLine(p1, p2);
        Gizmos.DrawLine(p2, p3);
    }
}