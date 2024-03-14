namespace UnityBase.DesignPatterns.Builder
{
    public struct EnemyData
    {
        public string Name { get; private set; }
        public int Health { get; private set; }
        public float Speed { get; private set; }
        public int Damage { get; private set; }
        public bool IsBoss { get; private set; }
    
        public struct Builder
        {
            private string name;
            private int health;
            private float speed;
            private int damage;
            private bool isBoss;
    
            public Builder WithName(string name)
            {
                this.name = name;
                return this;
            }
            
            public Builder WithHealth(int health)
            {
                this.health = health;
                return this;
            }
            
            public Builder WithSpeed(float speed)
            {
                this.speed = speed;
                return this;
            }
            
            public Builder WithDamage(int damage)
            {
                this.damage = damage;
                return this;
            }
            
            public Builder WithIsBoss(bool isBoss)
            {
                this.isBoss = isBoss;
                return this;
            }
    
            public EnemyData Build()
            {
                var theEnemy = new EnemyData
                {
                    Name = name,
                    Health = health,
                    Speed = speed,
                    Damage = damage,
                    IsBoss = isBoss
                };
    
                return theEnemy;
            }
        }
    }   
}