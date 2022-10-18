using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DndCore.Map;
using Logic.Core.Battle;
using Logic.Core.Graph;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject SelectionTile;
    public GameObject MapRoot;
    public GameObject Ball;
    public GameObject GreenBall;
    public GameObject InitiativeIndicator;

    public TextMeshProUGUI Log;

    List<SpriteManager> tiles = new List<SpriteManager>();
    public GameManager GameManager;
    public TextMeshPro Feedback;
    public Transform player;

    public void ResetUI()
    {
        InitiativeIndicator.transform.localScale = Vector3.zero;
    }

    public IEnumerator DrawMap(IMap map)
    {
        tiles.Clear();
        ////Log.text += "\nInit drawing";
        for (int x = 0; x < map.Height; x++)
        {
            yield return null;
            for (int y = 0; y < map.Width; y++)
            {
                var cell = map.GetCellInfo(y, x);
                if (cell.Terrain == ' ')
                {
                    continue;
                }

                GameObject go = Instantiate(SelectionTile);
                go.transform.parent = MapRoot.transform;
                go.transform.localScale = Vector3.zero;
                var delta = 0.0f;
                if ((((int)cell.Height) % 5) == 1)
                {
                    delta -= 0.0045f;
                }

                if ((((int)cell.Height) % 5) == 2)
                {
                    delta -= 0.0043f;
                }

                if ((((int)cell.Height) % 5) == 3)
                {
                    delta -= 0.00155f;
                }

                if ((((int)cell.Height) % 5) == 4)
                {
                    delta += 0.0035f;
                }

                go.transform.localPosition = new Vector3(0.5f + 0.0125f - cell.X * 0.025f,
                    ((int)cell.Height) * 0.01f + 0.0016f + delta, 0.5f + 0.0125f - cell.Y * 0.025f);
                //go.transform.GetComponent<MeshCollider>().enabled = false;
                var sprite = go.transform.GetComponent<SpriteManager>();
                sprite.X = x;
                sprite.Y = y;
                tiles.Add(sprite);
            }
        }

        InitiativeIndicator.transform.localScale = new Vector3(0.05f, -0.1f, 0.05f);
    }

    internal void HighlightCells(List<CellInfo> reachableCells)
    {
        foreach (var tile in tiles)
        {
            if (reachableCells.Any(res => res.X == tile.Y && res.Y == tile.X))
            {
                tile.transform.localScale = Vector3.one * 0.0025f;
            }
        }
    }

    internal void HighlightMovement(List<MemoryEdge> result)
    {
        foreach (var tile in tiles)
        {
            if (result.Any(res => res.Destination.X == tile.Y && res.Destination.Y == tile.X && res.Speed > 0))
            {
                tile.transform.localScale = Vector3.one * 0.0025f;
            }
        }
    }

    internal void ResetCellsUI()
    {
        foreach (var tile in tiles)
        {
            tile.transform.localScale = Vector3.zero;
        }
    }

    internal IEnumerator ShowGameEvents(List<GameEvent> events)
    {
        var gos = GameObject.FindGameObjectsWithTag("Creature");
        var creatures = gos.Select(x => { return x.GetComponent<CharacterQuantization>(); });
        var creatureInTurn = creatures.FirstOrDefault(x => x.Creature.Id == GameManager.Battle.GetCreatureInTurn().Id);
        if (events != null)
        {
            foreach (var eve in events)
            {
                if (eve.Type == GameEvent.Types.Movement)
                {
                    var tile = tiles.First(tile => tile.X == eve.Destination.Y && tile.Y == eve.Destination.X);
                    var target = tile.transform.localPosition;
                    yield return StartCoroutine(MoveToIterator(creatureInTurn.gameObject,
                        creatureInTurn.gameObject.transform.localPosition,
                        target,
                        0.25f
                    ));
                }

                if (eve.Type == GameEvent.Types.Attacks || eve.Type == GameEvent.Types.Spell)
                {
                    GameObject target = creatures.FirstOrDefault(x => x.Creature.Id == eve.Attacked).gameObject;
                    GameObject start = creatures.FirstOrDefault(x => x.Creature.Id == eve.Attacker).gameObject;
                    if (start != null && target != null)
                    {
                        GameObject ball = Instantiate(Ball);
                        ball.transform.parent = start.transform.parent;
                        yield return StartCoroutine(Move(ball,
                            start.transform.localPosition + new Vector3(0f, 0.01f, 0f),
                            target.transform.localPosition + new Vector3(0f, 0.01f, 0f), 1.0f));
                        yield return StartCoroutine(ShowText(target, "-" + eve.Damage, 1.0f));
                        Destroy(ball);
                    }
                }

                if (eve.Type == GameEvent.Types.Falling)
                {
                    GameObject ball = Instantiate(Ball);
                    ball.transform.parent = creatureInTurn.gameObject.transform.parent;
                    yield return StartCoroutine(Move(ball,
                        creatureInTurn.gameObject.transform.localPosition + new Vector3(0f, 0.01f, 0f),
                        creatureInTurn.gameObject.transform.localPosition + new Vector3(0f, 0.03f, 0f), 1.0f));
                    yield return StartCoroutine(ShowText(creatureInTurn.gameObject, "-" + eve.Damage, 1.0f));
                    Destroy(ball);
                }

                if (eve.Type == GameEvent.Types.SelfAbility)
                {
                    yield return StartCoroutine(ShowText(creatureInTurn.gameObject, eve.Ability, 1.0f));
                }

                if (eve.Type == GameEvent.Types.AttackMissed)
                {
                    GameObject target = creatures.FirstOrDefault(x => x.Creature.Id == eve.Attacked).gameObject;
                    GameObject start = creatures.FirstOrDefault(x => x.Creature.Id == eve.Attacker).gameObject;
                    if (start != null && target != null)
                    {
                        GameObject ball = Instantiate(Ball);
                        ball.transform.parent = start.transform.parent;
                        yield return StartCoroutine(Move(ball,
                            start.transform.localPosition + new Vector3(0f, 0.01f, 0f),
                            target.transform.localPosition + new Vector3(0f, 0.01f, 0f), 1.0f));
                        yield return StartCoroutine(ShowText(target, "MISS", 1.0f));
                        Destroy(ball);
                    }
                }

                yield return null;
            }
        }
    }

    private IEnumerator MoveToIterator(GameObject go, Vector3 start, Vector3 end, float time)
    {
        var now = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup - now < time)
        {
            var newPos = Vector3.Lerp(start, end, (Time.realtimeSinceStartup - now) / time);
            go.transform.localPosition = newPos;
            yield return null;
        }
    }

    private IEnumerator ShowText(GameObject target, string text, float time)
    {
        Feedback.transform.eulerAngles = player.eulerAngles;
        Feedback.transform.parent = target.transform;
        Feedback.transform.localPosition = Vector3.up * 0.05f;
        Feedback.text = Regex.Replace(text, "(\\B[A-Z])", " $1");
        var now = Time.realtimeSinceStartup;
        Feedback.alpha = 0.0f;
        while (Time.realtimeSinceStartup - now < time)
        {
            var newAlpha = (Time.realtimeSinceStartup - now) / time;
            Feedback.alpha = newAlpha;
            yield return null;
        }

        Feedback.alpha = 0.0f;
    }

    private IEnumerator Move(GameObject go, Vector3 start, Vector3 end, float time)
    {
        var now = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup - now < time)
        {
            var newPos = Vector3.Lerp(start, end, (Time.realtimeSinceStartup - now) / time);
            go.transform.localPosition = newPos;
            yield return null;
        }
    }

    void Update()
    {
        if (GameManager.Battle == null)
        {
            return;
        }

        var inTurn = GameManager.Battle.GetCreatureInTurn();
        if (inTurn == null)
        {
            return;
        }

        var gos = GameObject.FindGameObjectsWithTag("Creature");
        if (gos == null || gos.Count() == 0)
        {
            return;
        }

        var creatures = gos.Select(x => { return x?.GetComponent<CharacterQuantization>(); }).Where(x => x != null);
        var creatureInTurn =
            creatures.FirstOrDefault(x => x != null && x.Creature != null && x.Creature.Id == inTurn.Id);
        if (creatureInTurn != null)
        {
            InitiativeIndicator.transform.localPosition =
                creatureInTurn.transform.localPosition + new Vector3(0f, 0.05f, 0f);
        }
    }

    internal void ShowPath(List<CellInfo> cellPath, MemoryEdge end, Material color)
    {
        Log.text += "\nShowPath";
        foreach (var tile in tiles)
        {
            if (cellPath.Any(res => res.X == tile.Y && res.Y == tile.X) ||
                (end.Destination.X == tile.Y && end.Destination.Y == tile.X))
            {
                tile.Knob.gameObject.SetActive(true);
                tile.Knob.material = color;
            }
            else
            {
                tile.Knob.gameObject.SetActive(false);
            }
        }
    }
}