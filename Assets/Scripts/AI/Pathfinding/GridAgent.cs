using UnityEngine;
using System.Collections;
using UnityEditor.Experimental.GraphView;

public class Agent : MonoBehaviour
{

    const float minPathUpdateTime = .2f;
    const float pathUpdateMoveThreshold = .5f;

    public GameObject target;
    public float speed = 20;
    public float turnSpeed = 3;
    public float turnDst = 5;
    public float stoppingDst = 10;

    GridPath path;

    void Start()
    {
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

    IEnumerator UpdatePath()
    {

        if (Time.timeSinceLevelLoad < .3f)
        {
            yield return new WaitForSeconds(.3f);
        }
        PathRequestManager.RequestPath(new PathRequest(transform.position, target.transform.position, OnPathFound));

        float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
        Vector3 targetPosOld = target.transform.position;

        while (true)
        {
            yield return new WaitForSeconds(minPathUpdateTime);
            //print(((target.position - targetPosOld).sqrMagnitude) + "    " + sqrMoveThreshold);
            if ((target.transform.position - targetPosOld).sqrMagnitude > sqrMoveThreshold)
            {
                PathRequestManager.RequestPath(new PathRequest(transform.position, target.transform.position, OnPathFound));
                targetPosOld = target.transform.position;
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
    public void GoTo(Vector3 where)
    {
        target.transform.position = where;
        StartCoroutine(UpdatePath());
    }


    public Unit[] EnemiesAvailable()
    {
        return PathRequestManager.FindEnemiesAvailable(transform.position, GetComponent<Unit>().MoveRange, GetComponent<Unit>().AttackRange);
    }

}
}
