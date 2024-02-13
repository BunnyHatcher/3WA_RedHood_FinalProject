using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

    namespace Hobgoblin
{
    public class MoveHobgoblin : MonoBehaviour
    {

        #region References to other Classes
        public Transform _target;
        private NavMeshAgent _navAgent;   
        #endregion



        // Start is called before the first frame update
        void Start()
        {
        
        }

        private void Awake()
        {
            _navAgent = GetComponent<NavMeshAgent>();
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
