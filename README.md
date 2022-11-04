# 2D Collision Detection

这个库自制的 2D 碰撞场，用于做 2D 图形碰撞检测和物理模拟。

## 已经实现的特性

1. 任意形状的凸多边形的碰撞检测
2. 碰撞体的约束解算
3. 多等级的碰撞体阻挡规则
4. 基本的速度、加速度概念

[-> 查看 API 文档](./docs/html/index.html)

[-> 查看功能介绍](https://docs.sofunny.io/display/~yelingfeng/Feature)

## 性能测试

1. 在所有碰撞完全重叠，保证函数在 6ms 以内，可以同时处理以下数量级碰撞体
   1. 100 个圆形
   2. 44 个正方形
   3. 33 个不规则八边形
2. 在所有碰撞完全重叠，保证函数在 12ms 以内，可以同时处理以下数量级碰撞体
   1. 140 个圆形
   2. 60 个正方形
   3. 44 个八边形


[-> 查看性能测试](https://docs.sofunny.io/pages/viewpage.action?pageId=78833917)

## 如何调试

- 入口场景：Assets/Scenes/SampleScene.unity
- 入口脚本：Scripts/PhysicsWorld.cs
- 入口函数：void Start()
- 调试方法：通过编写 TestX 函数，在 Start 中执行
- 进入 play 模式后，可找到场景 CollisionObjectProxy 组件，修改其 isInControl 属性进行键盘控制