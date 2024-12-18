using UnityEngine;
using System.Collections;
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using UnityEngine.UIElements;
using Unity.VisualScripting;

public class Agent : MonoBehaviour
{

    const float minPathUpdateTime = .2f;
    const float pathUpdateMoveThreshold = .5f;

    public Vector3 target;
    public GameObject sprite;
    public float speed = 20;
    public float turnSpeed = 3;
    public float turnDst = 5;
    public float stoppingDst = 10;

    GridPath path;

    GridPath distancePath;

    void Start()
    {
        target = transform.position;
        StartCoroutine(UpdatePath());
    }

    public void OnPathFound(Vector3[] waypoints, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            path = new GridPath(waypoints, transform.position, turnDst, stoppingDst);

            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }

    //LO USAMOS PARA CALCULAR DISTANCIAS SIN RECORRER LA RUTA
    public void DistanceOnPathFound(Vector3[] waypoints, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            distancePath = new GridPath(waypoints, transform.position, turnDst, stoppingDst);
        }
    }

    IEnumerator UpdatePath()
    {

        if (Time.timeSinceLevelLoad < .3f)
        {
            yield return new WaitForSeconds(.3f);
        }
        PathRequestManager.RequestPath(new PathRequest(transform.position, target, OnPathFound));

        float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
        Vector3 targetPosOld = target;

        while (true)
        {
            yield return new WaitForSeconds(minPathUpdateTime);
            //print(((target.position - targetPosOld).sqrMagnitude) + "    " + sqrMoveThreshold);
            if ((target - targetPosOld).sqrMagnitude > sqrMoveThreshold)
            {
                PathRequestManager.RequestPath(new PathRequest(transform.position, target, OnPathFound));
                targetPosOld = target;
            }
        }
    }

    IEnumerator FollowPath()
    {

        bool followingPath = true;
        int pathIndex = 0;
        transform.LookAt(path.lookPoints[0]);

        float speedPercent = 1;

        //Are we going down?
        bool down = false;

        while (followingPath)
        {
            Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);
            while (path.turnBoundaries[pathIndex].HasCrossedLine(pos2D))
            {
                if (pathIndex == path.finishLineIndex)
                {
                    followingPath = false;
                    break;
                }
                else
                {
                    pathIndex++;
                }
            }

            

            if (followingPath)
            {
                //Let's only go up if the next point is heigher
                if (path.lookPoints[pathIndex].y == transform.position.y)
                {
                    if (pathIndex >= path.slowDownIndex && stoppingDst > 0)
                    {
                        speedPercent = Mathf.Clamp01(path.turnBoundaries[path.finishLineIndex].DistanceFromPoint(pos2D) / stoppingDst);
                        if (speedPercent < 0.01f)
                        {
                            followingPath = false;
                        }
                    }
                    //Rotate Agent towards Look Points (Let's avoid changing x and z rotations)
                    Quaternion targetRotation = Quaternion.LookRotation(path.lookPoints[pathIndex] - transform.position);

                    targetRotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0); // Let's only modify the y rotation


                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);

                    //Move Agent in the correct position
                    transform.Translate(Vector3.forward * Time.deltaTime * speed * speedPercent, Space.Self);

                     //Esto es para que se flipee el sprite cuando toca
                    if (sprite != null){

                        SpriteRenderer spriteRenderer = sprite.GetComponent<SpriteRenderer>();


                        if (DoesVectorPointToTheRight(transform.forward) == true) {
                            //sprite.transform.rotation = Quaternion.Euler(new Vector3(60, 0, 0));
                            spriteRenderer.flipX = false;
                        }

                        else{
                            //sprite.transform.rotation = Quaternion.Euler(new Vector3(-60, 180, 0));
                            spriteRenderer.flipX = true;
                        }
                    }

                }
                else if(path.lookPoints[pathIndex].y > transform.position.y && !down) { //Basic movement up
                    transform.Translate(Vector3.up * Time.deltaTime * speed * speedPercent, Space.Self);
                }
                else if (path.lookPoints[pathIndex].y < transform.position.y)
                { //Basic movement down
                    
                    transform.Translate(Vector3.down * Time.deltaTime * speed * speedPercent, Space.Self);
                    down = true;
                }
                else if (down && path.lookPoints[pathIndex].y > transform.position.y)
                {
                    transform.position = new Vector3(transform.position.x, path.lookPoints[pathIndex].y, transform.position.z);
                    down = false;
                }

            }
            
            yield return null;

        }

        
    }

    public void OnDrawGizmos()
    {
        if (path != null)
        {
            path.DrawWithGizmos();
        }
    }


    //Usable Methods

    public bool HasPath() //new
    {
        return path != null;
    }

    public bool HasReachedDestination() //new
    {
        return path != null && path.finishLineIndex == path.turnBoundaries.Length - 1;
    }

    public void GoTo(Vector3 where)
    {
        target = where;
        StartCoroutine(UpdatePath());
    }

    public bool IsPlaceAvailable(Vector3 punto, float range)
    {
        return PathRequestManager._IsPlaceAvailable(punto, range);
    }

    public Unit[] EnemiesAvailable()
    {
        return PathRequestManager.FindEnemiesAvailable(transform.position, GetComponent<Unit>().MoveRange, GetComponent<Unit>().AttackRange);
    }

    //new functions for findbestattack and findsafeplaces

    public GridNode[] FindBestAttackPlaces(Unit targetUnit, List<Unit> enemies, UnitType unitType)
    {
        return PathRequestManager.FindBestAttackPlaces(transform.position, GetComponent<Unit>().MoveRange, targetUnit, enemies, unitType);
    }

    public GridNode[] FindSafePlaces(List<Unit> enemies)
    {
        return PathRequestManager.FindSafePlaces(transform.position, GetComponent<Unit>().MoveRange, enemies); 
    }

    public float DistanceBetweenTwoNodes(Vector3 n1, Vector3 n2)   //DISTANCIA ENTRE DOS NODOS REAL, USANDO RUTAS
    {
        Vector3[] path = PathRequestManager.PathBeetweenNodes(n1,n2);
        /*
        float time = 0;
        while(distancePath == null)
        {
            time += Time.deltaTime;
            if(time >= 3f)
            {
                break;
            }
        }
        */
        float distance = 0f;
        for (int i = 0; i < path.Length - 1; i++)
        {
            //Debug.Log("ENTRAMOS EN EL BUCLE");
            distance = distance + Vector3.Distance(path[i], path[i + 1]);
        }
        return distance;
    }

    private bool DoesVectorPointToTheRight(Vector3 vector)
    {
        // Obtén el vector forward del objeto
        Vector3 forward = vector;

        // Proyección del vector forward sobre el eje X
        float forwardX = Vector3.Dot(forward, Vector3.right);

        if (forwardX >= 0)
        {
            return true;
        }
        else
        {
           return false;
        }
    }

    
}
/*GameObject[] GetEnemiesInRange( Unit UnidadSeleccionada, GameObject[] listaUnidadesJugador) {

    //nodoUnidadSeleccionada: Unidad controlada por la ia de la que quieres comprobar a quién puede atacar
    //listaUnidadesJugador: lista con todas las unidades VISIBLES* del jugador  *(si al final pasamos de la niebla de guerra, ignorar lo de visibles)


    //para cada iteración, crea un clon, le hace moverse la distancia máxima haca el enemigo, y entonces hace un raycast para ver si hay un obstáculo 
    //entre medias, si da, entonces se añade ese enemigo al array de enemigosAtacables, y si no, pues nada 

    float attackRange = UnidadSeleccionada.AttackRange;
    float attackDamage = UnidadSeleccionada.AttackDamage;

    foreach (Unit in UnidadSeleccionada)
    {

    }
}*/

