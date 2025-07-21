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
    this.onWordChoicesAvailableCallback = null; // 画师接收可选词语的回调
    this.onGameStateUpdatedCallback = null;     // 所有玩家接收游戏状态更新的回调

    // 存储计时器更新回调函数
    this.timerUpdateCallback = null;

        // 新增：事件缓存（存储未处理的事件）
    this.eventCache = {
      WordChoicesAvailable: null,  // 缓存词语选项事件
      GameStateUpdated: null        // 缓存游戏状态更新事件（可能有多个）
    };
  }


  // 启动回合计时器（修复连接状态判断）
  async StartRoundWithTimer() {
    console.log('[SignalR] 准备启动计时器');

    // 严格检查连接状态：必须已初始化且状态为Connected
    if (!this.hubConnection) {
      console.error('[SignalR] 连接未初始化，无法启动计时器');
      return false;
    }
    if (this.hubConnection.state !== signalR.HubConnectionState.Connected) {
      console.error(`[SignalR] 连接状态异常（${this.hubConnection.state}），无法启动计时器`);
      return false;
    }

    try {
      await this.hubConnection.invoke('StartRoundWithTimer', this.currentRoomId.value);
      console.log(`[SignalR] 已向房间${this.currentRoomId.value}发送启动计时器请求`);
      return true;
    } catch (error) {
      console.error('[SignalR] 启动计时器失败:', error);
      return false;
    }
  }



  // 设置计时器监听器
  setupTimerListener(callback){
    this.timerUpdateCallback = callback;

    if(this.hubConnection){
      // 监听来自服务端的TimerUpdate事件
      this.hubConnection.on('TimerUpdate',(remainingSeconds) => {
        // console.log(remainingSeconds);
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
  registerWordChoicesCallback(callback) {
    this.onWordChoicesAvailableCallback = callback;
    console.log('[SignalR] 已注册词语选项回调');
    if (this.eventCache.WordChoicesAvailable) {
      console.log('[SignalR] 补发缓存的词语选项事件');
      // 复用上面的事件处理逻辑（避免代码重复）
      this.onWordChoicesAvailableCallback({
        choices: this.eventCache.WordChoicesAvailable.choices,       // 可选词语列表（如 ["苹果", "香蕉"]）
        tip: this.eventCache.WordChoicesAvailable.tip || '请选择一个词语进行绘画', // 提示信息
        painterUserId: this.eventCache.WordChoicesAvailable.painterUserId // 验证当前用户是否为画师（可选）
      });
    }
  }
  registerGameStateUpdatedCallback(callback) {
    this.onGameStateUpdatedCallback = callback;
    console.log('[SignalR] 已注册游戏状态更新回调');
    if (this.eventCache.GameStateUpdated) {
      console.log('[SignalR] 补发最新的游戏状态事件');
      // 复用事件处理逻辑，传入最新的缓存数据
      this.onGameStateUpdatedCallback({
        currentRound: this.eventCache.GameStateUpdated.currentRound,         // 当前回合（如 1）
        totalRounds: this.eventCache.GameStateUpdated.totalRounds,           // 总回合数（如 5）
        currentPainter: {                        // 当前画师信息
          userId: this.eventCache.GameStateUpdated.currentPainterUserId,
          username: this.eventCache.GameStateUpdated.currentPainterUsername // 可选：后端可返回用户名
        },
        currentPhase: this.eventCache.GameStateUpdated.currentPhase,     // 游戏阶段（如 "SelectingWord"、"DrawingAndGuessing"）
        // 其他需要的状态数据（如玩家分数、剩余时间等）
        playerScores: this.eventCache.GameStateUpdated.playerScores || []    // 可选：玩家分数列表
      });
    }
  }
  // 初始化并启动连接
  async initialize(roomId) { // 默认为房间1
    this.currentRoomId.value = roomId;
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`/gameHub?roomId=${roomId}`)
      .withAutomaticReconnect()
      .build();
    //this.isConnected.value = true;
    console.log(`[SignalR] 正在连接到房间${roomId}...`);
    this.hubConnection.on('GameStarted', (data) => {  // 正确使用 this.hubConnection
      console.log('[SignalR] 收到游戏开始通知:', data);
      // 如果需要通过回调通知组件，可以添加一个 gameStartedCallback
      if (this.gameStartedCallback && typeof this.gameStartedCallback === 'function') {
        this.gameStartedCallback(data);
      }
    });
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

    this.hubConnection.on('WordChoicesAvailable', (data) => {
    console.log('[SignalR] 收到词语选项数据:', data);
    // 验证数据结构
    if (!data || !data.choices || !Array.isArray(data.choices)) {
      console.error('[SignalR] 词语选项数据格式错误:', data);
      return;
    }
    this.eventCache.WordChoicesAvailable = data; // 缓存事件
    // 调用回调函数通知前端组件（如画师的选词界面）
    if (this.onWordChoicesAvailableCallback && typeof this.onWordChoicesAvailableCallback === 'function') {
      this.onWordChoicesAvailableCallback({
        choices: data.choices,       // 可选词语列表（如 ["苹果", "香蕉"]）
        tip: data.tip || '请选择一个词语进行绘画', // 提示信息
        painterUserId: data.painterUserId // 验证当前用户是否为画师（可选）
      });
    } else {
      console.log('[SignalR] 词语选项回调未注册，缓存事件');
    }
  });


  // 新增：监听 GameStateUpdated 事件（所有玩家都会收到）
  this.hubConnection.on('GameStateUpdated', (data) => {
    console.log('[SignalR] 收到游戏状态更新:', data);
    // 验证核心数据
    if (data.currentRound === undefined || data.currentPhase === undefined) {
      console.error('[SignalR] 游戏状态数据格式错误:', data);
      return;
    }
    console.log('现在在signalRService的GameStateUpdated函数中，接收到游戏状态更新:', data);
    this.eventCache.GameStateUpdated = data; // 只保留最新的状态
    // 调用回调函数通知所有前端组件（如游戏主界面）
    if (this.onGameStateUpdatedCallback && typeof this.onGameStateUpdatedCallback === 'function') {
      this.onGameStateUpdatedCallback({
        currentRound: data.currentRound,         // 当前回合（如 1）
        totalRounds: data.totalRounds,           // 总回合数（如 5）
        currentPainter: {                        // 当前画师信息
          userId: data.currentPainterUserId,
          username: data.currentPainterUsername // 可选：后端可返回用户名
        },
        currentPhase: data.currentPhase,     // 游戏阶段（如 "SelectingWord"、"DrawingAndGuessing"）
        // 其他需要的状态数据（如玩家分数、剩余时间等）
        playerScores: data.playerScores || []    // 可选：玩家分数列表
      });
    } else {
      console.log('[SignalR] 游戏状态回调未注册，缓存事件');
    }
  });

    // // 2. 注册游戏开始成功的回调
    // connection.on("GameStarted", (data) => {
    //     console.log("游戏已开始:", data.message);

    // });
    // 处理连接状态变化
    this.hubConnection.onclose(() => {
      this.isConnected.value = false;
      console.log('SignalR 连接已断开');
    });

    this.hubConnection.onreconnecting((error) => {
      this.isConnected.value = false;
      console.log('[SignalR] 正在重连...', error?.message || '');
    });

    this.hubConnection.onreconnected((connectionId) => {
      this.isConnected.value = true;
      this.connectionId.value = connectionId; // 重连后更新连接ID
      console.log(`[SignalR] 重连成功，新连接ID: ${connectionId}`);
      // 重连后自动加入组（仅当之前成功加入过）
      if (this.currentRoomId.value && this._lastPlayerIdForJoin) {
        this.joinGroup(this.currentRoomId.value, this._lastPlayerIdForJoin)
          .catch(err => console.warn('[SignalR] 重连后加入组失败:', err));
      }
    });


    try {
      // 启动连接（首次连接）
      await this.hubConnection.start();
      this.isConnected.value = true;
      this.connectionId.value = this.hubConnection.connectionId;
      console.log(`[SignalR] 首次连接成功（房间${roomId}），连接ID: ${this.connectionId.value}`);
      return true;
    } catch (error) {
      console.error(`[SignalR] 首次连接失败（房间${roomId}）:`, error);
      this.hubConnection = null; // 连接失败时清空实例
      return false;
    }
  }

    // 发送聊天消息
  async confirmWordSelection(word) {
    if (!this._isConnectionReady()) return false;
    try {
      await this.hubConnection.invoke('ConfirmWordSelection', this.currentRoomId.value, word);
      return true;
    } catch (error) {
      console.error('[SignalR] 发送选择的词语失败:', error);
      return false;
    }
  }
  // 发送聊天消息
  async sendChatMessage(message) {
    if (!this._isConnectionReady()) return false;
    try {
      await this.hubConnection.invoke('SendChatMessage', this.currentRoomId.value, message);
      return true;
    } catch (error) {
      console.error('[SignalR] 发送消息失败:', error);
      return false;
    }
  }

  // 加入房间组（关键：仅在首次连接或重连后调用）
  async joinGroup(groupId, playerId) {
    if (!this.hubConnection || this.hubConnection.state !== signalR.HubConnectionState.Connected) {
      console.error('[SignalR] 连接未就绪，无法加入组');
      return false;
    }

    try {
      await this.hubConnection.invoke('AddToConnectionMap', this.connectionId.value, playerId);
      await this.hubConnection.invoke('JoinRoom', groupId, playerId);
      this._lastPlayerIdForJoin = playerId; // 保存玩家ID用于重连
      console.log(`[SignalR] 成功加入组（房间${groupId}，玩家${playerId}）`);
      return true;
    } catch (error) {
      console.error(`[SignalR] 加入组失败（房间${groupId}）:`, error);
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

  async sendStroke(strokeData) {
    if (!this._isConnectionReady()) return false;
    try {
      await this.hubConnection.invoke('SendStroke', this.currentRoomId.value, strokeData);
      return true;
    } catch (error) {
      console.error('[SignalR] 发送绘画数据失败:', error);
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
  // 辅助方法：检查连接是否就绪（统一判断逻辑）
  _isConnectionReady() {
    if (!this.hubConnection) {
      console.error('[SignalR] 连接未初始化');
      return false;
    }
    if (this.hubConnection.state !== signalR.HubConnectionState.Connected) {
      console.error(`[SignalR] 连接状态异常（${this.hubConnection.state}），无法执行操作`);
      return false;
    }
    if (!this.isConnected.value) {
      console.error('[SignalR] 连接未就绪（isConnected为false）');
      return false;
    }
    return true;
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