using UnityEngine;

namespace TylerKozaki.Objects
{
    public class Smokey : FPBaseObject
    {
        public static int classID = -1;
        private Vector2 start;
        private Animator animator;
        private float genericTimer = 0f;

        private new void Start()
        {
            start.x = transform.position.x;
            start.y = transform.position.y;
            terrainCollision = true;
            animator = GetComponent<Animator>();
            base.Start();
            classID = FPStage.RegisterObjectType(this, GetType(), 64);
            objectID = classID;
            gameObject.SetActive(false);
        }

        public override void ResetStaticVars()
        {
            base.ResetStaticVars();
            classID = -1;
        }

        public override void ObjectCreated()
        {
            activationMode = FPActivationMode.ALWAYS_ACTIVE;
            animator.Play("Smoke", -1, 0f);
            genericTimer = 0f;
        }

        private void Update()
        {
            genericTimer += FPStage.deltaTime;
            position += new Vector2(0f, FPStage.deltaTime);
            if (genericTimer > 30f)
            {
                FPStage.DestroyStageObject(this);
            }
        }
    }
}
