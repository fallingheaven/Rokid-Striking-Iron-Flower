## 摄像机

### 官方预制体

​	搜索RKCameraRig

### 组件

#### RK Camera Rig

​	设置自由度和渲染更新频率

#### RK Camera Setting

​	日志和编辑器模式支持

#### Device EventHandler

​	rokid设备设置

---

## UI

### 需要知道的

unity空间中1Unit（一个格子）大致相当于1米。

### 如何显示UI

和其他UI不同，Rokid的UI建议不使用Screen Space，而是使用World Space

画布下挂载UI Overlay和Follow Camera组件来实现固定位置

---

## 交互

### 官方预制体

​	[RKInput]：输入管理器
​	PointableUI：可交互、可点击UI

### 组件

#### InputManager

​	改变输入方式，包括：
​		控制器3DoF射线交互
​		TouchPad交互（需要**Station2空间计算设备**）
​		手势交互
​		鼠标交互

​	输入的流程：输入源 → 指针事件PointerEvent → IPointable.WhenPointerEventRaised委托 → IPointableElement → IPointableCanvas → Unity UI事件系统

​	

#### Pointable Canvas

​	所属画布模块，

#### Ray Interactable

#### Poke Interactable

#### BoxCollider Size Fit To Canvas

---

### 模块构建

#### 画布

##### 交互逻辑

IPointable -> IPointableElement -> IPointableCanvas

##### 画布网格

```mer

```

