// 默认连接到房间1！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！
//！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！
// src/services/signalRService.js
import * as signalR from '@microsoft/signalr';
import { ref, computed, onMounted, onUnmounted } from 'vue';

class SignalRService {
  constructor() {
    this.hubConnection = null;
    this.chatMessages = ref([]);
    this.connectionId = ref('');
    this.isConnected = ref(false);
    this.currentRoomId = ref(1); // 默认连接到房间1
    this.receivedStrokes = ref([]); // 用于存储收到的所有笔画

    // 添加回调函数属性
    this.onStrokeReceivedCallback = null;
    this.onRedoReceivedCallback = null;
    this.onUndoReceivedCallback = null;
    this.onClearReceivedCallback = null;

    // 存储计时器更新回调函数
    this.timerUpdateCallback = null;
  }


  // 启动回合计时器 //这个函数有问题
  async StartRoundWithTimer(){
    console.log('现在在signalRService的StartRoundWithTimer函数中,准备启动计时器');

    // 在这个地方检查signalR的连接状态
    if(!this.hubConnection){
      console.error('SignalR连接尚未初始化');
      return;
    }

    console.log('当前连接状态',this.hubConnection.state); //显示出连接状态  //connecting

    try{
      if(this.hubConnection) //&& this.hubConnection.state == signalR.HubConnectionState.Connected){
      { //console.log('this.currentRoomId.value type:',typeof this.currentRoomId)
        console.log('我到了if语句里面'); 


        //我需要在这里检查我的连接状态
        console.log('当前连接状态2:', this.hubConnection.state);


        await this.hubConnection.invoke('StartRoundWithTimer',this.currentRoomId.value);
        console.log('启动${this.currentRoomId.value}的计时器');
      }else{
        console.error('SignalR连接未建立或已断开');
      }
    }
    catch(error){
      console.log('寄了');
      console.error('启动计时器失败',error);
    }
  }



  // 设置计时器监听器
  setupTimerListener(callback){
    this.timerUpdateCallback = callback;

    if(this.hubConnection){
      // 监听来自服务端的TimerUpdate事件
      this.hubConnection.on('TimerUpdate',(remainingSeconds) => {
        console.log('接收到计时器更新:,${remainingSeconds}秒');
        this.onTimerUpdate(remainingSeconds);
      });
    }
    else{
      console.error('SignalR连接未建立,无法设置计时器监听');
    }
  }

  // 处理计时器更新
  onTimerUpdate(remainingSeconds){
    console.log('现在在singalRService的onTimerUpdate函数中，接收到剩余时间:',remainingSeconds);
    if(this.timerUpdateCallback && typeof this.timerUpdateCallback === 'function'){
      this.timerUpdateCallback(remainingSeconds);
    }else{
      console.warn('计时器更新回调函数未设置');
    }
  }

  // 移除计时器监听器（清理用）
  removeTimerListener(){
    if(this.hubConnection){
      this.hubConnection.off('TimerUpdate');
      this.timerUpdateCallback = null;
      console.log('计时器监听器已经移除');
    }
  }

  // 注册接收绘画数据的回调函数
  registerStrokeReceivedCallback(callback)
  {
    this.onStrokeReceivedCallback = callback;
    console.log('注册了registerStrokeReceivedCallback函数');
  }
  
  // 注册接收重做操作的回调函数
  registerRedoReceivedCallback(callback)
  {
    this.onRedoReceivedCallback = callback;
    console.log('注册了registerRedoReceivedCallback函数');
  }

  // 注册接收撤销操作的回调函数
  registerUndoReceivedCallback(callback)
  {
    this.onUndoReceivedCallback = callback;
    console.log('注册了registerUndoReceivedCallback函数');
  }

  // 注册接收清空操作的回调函数
  registerClearReceivedCallback(callback)
  {
    this.onClearReceivedCallback = callback;
    console.log('注册了registerClearReceivedCallback函数');
  }

  // 初始化并启动连接
  async initialize(roomId) { // 默认为房间1
    this.currentRoomId.value = roomId;
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`/gameHub?roomId=${roomId}`)
      .withAutomaticReconnect()
      .build();
    this.isConnected.value = true;
    // 注册接收消息的处理函数
    this.hubConnection.on('ReceiveChatMessage', (data) => {
      console.log('接收到消息:', data);
       console.log(this.chatMessages.value);
      // 验证数据格式
      if (!data || !data.content) {
        console.error('无效的消息格式:', data);
        return;
      }
      
      // 使用不可变方式更新数组（确保 Vue 检测到变化）
         this.chatMessages.value = [
        ...this.chatMessages.value,
        {
          playerId: data.playerId || 0,
          username: data.username || '匿名用户',
          content: data.content,  // 关键：使用 content 字段
          timestamp: data.timestamp || new Date().toISOString(),
          isSystem: false,
          isCorrect: data.isCorrect || false, // 默认值为 false
          isWrong: false,
          scores: data.scores || []
        }
      ];
    });
   

    // 注册接受绘画数据的处理函数
    /*********************************/
    // 现在已经可以正常传递到这个地方了
    /*********************************/
    this.hubConnection.on('ReceiveStroke', (data) => {
      console.log('现在在signalRService的ReceiveStroke函数中，接受到绘画数据：', data);
      // 将收到的笔画存入到 receivedStrokes 中
      if(data &&data.strokeData) {
        this.receivedStrokes.value.push(data.strokeData)
      
      // 调用回调函数通知DrawingBoard组件
      if(this.onStrokeReceivedCallback){
        this.onStrokeReceivedCallback(data.strokeData);
        console.log('调用了回调函数onStrokeReceivedCallback');
      }

      // 调试信息
      console.log('现在在signalRService的ReceiveStroke函数中，receivedStrokes已经修改');
      }
    });

    // 注册接收撤销操作的处理函数
    this.hubConnection.on('ReceiveUndo', (data) => {
      console.log('现在在signalRService的ReceiveUndo函数中,接收到撤销操作');

      //调试信息
      //console.log('现在我正要进入onUndoReceivedCallback函数');

      // 调用回调函数通知DrawingBoard组件
      if(this.onUndoReceivedCallback){
        //console.log('现在我进入了onUndoReceivedCallback函数');
        this.onUndoReceivedCallback(); // 这里显示我成功进来了
        console.log('调用了回调函数onUndoReceivedCallback');
      }
      // else
      // {
      //   console.log('现在我没有进入onUndoReceivedCallback函数');
      // }

      // 调试信息
      console.log('现在在signalRService的ReceiveUndo函数中'); // 在这个地方
    });

    // 注册接收重做操作的处理函数
    this.hubConnection.on('ReceiveRedo', (data) => {
      console.log('现在在signalRService的ReceiveRedo函数中，接收到重做操作');

      // 调用回调函数通知DrawingBoard组件
      if(this.onRedoReceivedCallback){
        this.onRedoReceivedCallback();
        console.log('调用了回调函数onRedoReceivedCallback');
      }

      // 调试信息
      console.log('现在在signalRService的ReceiveRedo函数中');
    })

    // 注册接收清空操作的处理函数
    this.hubConnection.on('ReceiveClear', (data) => {
      console.log('现在在signalRService的ReceiveClear函数中，接收到清空操作');

      // 调用回调函数通知DrawingBoard组件
      if(this.onClearReceivedCallback){
        this.onClearReceivedCallback();
        console.log('调用了回调函数onClearReceivedCallback');
      }

      // 调试信息
      console.log('现在在signalRService的ReceiveClear函数中');
    })

    // 处理连接状态变化
    this.hubConnection.onclose(() => {
      this.isConnected.value = false;
      console.log('SignalR 连接已断开');
    });

    this.hubConnection.onreconnected(async () => {
      this.isConnected.value = true;
      this.hubConnectionId.value = this.hubConnection.connectionId;
      console.log('SignalR 连接已重新建立');
      // 关键：重连后自动重新加入组
      if (this.currentRoomId.value && this._lastPlayerIdForJoin) {
        try {
          console.log(this.currentRoomId.value);
          console.log(this._lastPlayerIdForJoin);
          await this.joinGroup(this.currentRoomId.value, this._lastPlayerIdForJoin);
          console.log('SignalR 重连后已自动重新加入组');
        } catch (e) {
          console.warn('SignalR 重连后自动加入组失败:', e);
        }
      }
    });

    this.hubConnection.onreconnecting((error) => {
      this.isConnected.value = false;
      console.log('SignalR 正在重连:', error);
    });

    try {
      await this.hubConnection.start();
      this.isConnected.value = true;
      this.connectionId.value = this.hubConnection.connectionId;
      console.log('SignalR 连接已重新建立');
      // 关键：重连后自动重新加入组
       console.log(this.currentRoomId.value);
      console.log(this._lastPlayerIdForJoin);
      if (this.currentRoomId.value && this._lastPlayerIdForJoin) {
        try {
         
          await this.joinGroup(this.currentRoomId.value, this._lastPlayerIdForJoin);
          console.log('SignalR 重连后已自动重新加入组');
        } catch (e) {
          console.warn('SignalR 重连后自动加入组失败:', e);
        }
      }
      console.log(`SignalR 已连接到房间 ${roomId}`);
      return true;
    } catch (error) {
      console.error('SignalR 连接失败:', error);
      return false;
    }
  }

  // 发送聊天消息
  async sendChatMessage(message) {
    // 调试信息
    console.log(this.isConnected.value);
    console.log(this.hubConnection);
    if (!this.isConnected.value || !this.hubConnection) {
        console.error('SignalR 连接未建立');
        return false;
    }

    try {
        // 传递当前房间ID和消息内容（字符串）
        await this.hubConnection.invoke('SendChatMessage', this.currentRoomId.value, message);
        return true;
    } catch (error) {
        console.error('发送消息失败:', error);
        return false;
    }
  }

  // 加入SignalR组（房间），需要在连接建立后调用
  async joinGroup(groupId, playerId) {
    if (!this.isConnected.value || !this.hubConnection) {
      console.error('SignalR 连接未建立，无法加入组');
      return false;
    }
    try {
      // 先建立连接映射
      await this.hubConnection.invoke('AddToConnectionMap', this.hubConnection.connectionId, playerId);
      // 再加入房间
      await this.hubConnection.invoke('JoinRoom', groupId, playerId);
      // 记录最后一次加入组的playerId，便于重连后自动恢复
      this._lastPlayerIdForJoin = playerId;
      console.log('SignalR 连接已加入组:', this._lastPlayerIdForJoin);
      console.log(`已加入SignalR组: ${groupId}, 玩家ID: ${playerId}`);
      return true;
    } catch (error) {
      console.error('加入SignalR组失败:', error);
      return false;
    }
  }

  // 断开连接
  async disconnect() {
    // //计时器清理函数
    // this.removeTimerListener();

    if (this.hubConnection) {
      await this.hubConnection.stop();
      this.isConnected.value = false;
    }
  }

  // 切换房间
  async switchRoom(roomId) {
    if (this.isConnected.value) {
      await this.disconnect();
    }
    return await this.initialize(roomId);
  }

  // 发送绘画信息
  async sendStroke(strokeData) {
    // 调试信息
    console.log('现在在signalRService的sendStroke方法中，正在发送绘画数据:', strokeData);

    if(!this.isConnected.value || !this.hubConnection) {
      console.error('SignalR 连接未建立');
      return false;
    }

    try{
      // 传递当前房间ID和绘画数据
      await this.hubConnection.invoke('SendStroke', this.currentRoomId.value, strokeData);
      return true;
    } catch(error){
      console.error('发送绘画数据失败:', error);
      return false;
    }
  }

  // 发送撤销操作
  async sendUndo() {
    // 调试信息
    console.log('现在在signalRService的sendUndo方法中,正在发送撤销操作');
    
    if(!this.isConnected.value || !this.hubConnection) {
      console.error('SignalR 连接未建立');
      return false;
    }

    try{
      // 传递当前房间ID
      await this.hubConnection.invoke('SendUndo',this.currentRoomId.value);
      return true;
    } catch(error){
      console.error('发送撤销操作失败:', error);
      return false;
    }
  }

  // 发送重做操作
  async sendRedo() {
    // 调试信息
    console.log('现在在signalRService的sendRedo方法中，正在发送重做操作');

    if(!this.isConnected.value || !this.hubConnection) {
      console.error('SignalR 连接未建立');
      return false;
    }

    try{
      // 传递当前房间ID
      await this.hubConnection.invoke('SendRedo', this.currentRoomId.value);
      return true;
    } catch(error){
      console.error('发送重做操作失败:', error);
      return false;
    }
  }

  // 发送清空画布操作
  async sendClear(){
    // 调试信息
    console.log('现在在signalRService的sendClear方法中，正在发送清空画布操作');

    if(!this.isConnected.value || !this.hubConnection) {
      console.error('SignalR 连接未建立');
      return false;
    }

    try{
      // 传递当前房间ID
      await this.hubConnection.invoke('SendClear', this.currentRoomId.value);
      return true;
    } catch(error){
      console.error('发送清空画布操作失败:', error);
      return false;
    }
  }

}

// 创建单例实例
const signalRService = new SignalRService();

export default signalRService;