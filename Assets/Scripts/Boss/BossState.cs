namespace BossFSM
{
    public abstract class BossState
    {
        protected BossContext ctx;
        protected BossState(BossContext c) { ctx = c; }
        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Tick() { }
    }
}
