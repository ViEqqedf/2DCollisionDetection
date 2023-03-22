namespace CustomPhysics.Collision {
    public enum CollisionDirtyType {
        None = 1 << 0,
        Position = 1 << 1,
        Rotation = 1 << 2,
        Scale = 1 << 3,
    }
}