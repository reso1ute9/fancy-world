# fancy-world
multiplayer online game demo

---

### 游戏介绍
这是一款多人联机小demo，一名玩家可以在局域网内创建房间邀请其他玩家加入到游戏中，进入游戏时可以自定义玩家昵称和性别，不同性别对应不同角色模型，仅当所有玩家都准备就绪时房主才能开始游戏，进入游戏后所有玩家可以通过聊天框相互聊天。该项目主要是想尝试使用Unity NetCode实现多人联机功能，并将其添加到其他demo中，而且该项目也是本人了解接触Unity DOT的一个小案例

### 游戏Demo地址

[待补充]

### 游戏设计
由于小demo内容较少，因此按照不同场景对该项目进行介绍，游戏实现重点在于数据、动画同步，详细文档请见 **[fancy-world/Document](./Document/)**

**初始场景：** 空场景，挂载GameManager和NetworkManager，其中GameManager主要负责加载场景，缓存所有玩家数据

**开始场景：** 玩家可以在开始场景中输入IP地址创建房间或者是直接加入对应IP地址房间

**大厅场景：** 在大厅场景中玩家可以自定义自己昵称和选择角色性别，不同性别对应不同角色模型，当有玩家改变昵称、性别、准备状态时会在大厅中间信息框内同步更新，仅当所有玩家准备好后主机才可以开始游戏

**游戏场景：** 进入游戏后会在出生点附近一小块范围内随机设置玩家位置，当房间内某一名玩家移动时会将该玩家坐标和动画都同步到房间内所有玩家机器上，如果某个玩家在聊天框内发送消息，其他所有玩家均可收到对应消息
