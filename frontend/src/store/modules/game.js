// frontend/src/store/modules/game.js

const state = 
{
  isGameStarted: false,
  gameStatus: 'idle', // idle, playing, roundEnd, gameEnd
  currentRound: 0,
  totalRounds: 5,
  drawer: null, //当前绘画者
  timeRemaining:0, //剩余时间
  word: // 当前要猜测的词语
  {
    display: '',
    actual:'',
    hint:''
  },
  canvas:
  {
    drawHistory:[], //绘画历史记录
    currentDrawing: null //当前绘制的内容
  },
  guesses: [], //玩家猜词的记录
  roundResults: [], //各个回合的结果
  gameResults: null, //游戏结果
  timer: null //计时器引用
};

const mutations = 
{
  SET_GAME_STARTED(state, isStarted)
  {
    state.isGameStarted = isStarted
  },
  SET_GAME_STATUS(state, status)
  {
    state.gameStatus = status
  },
  SET_CURRENT_ROUND(state, round)
  {
    state.currentRound = round
  },
  SET_TOTAL_ROUNDS(state, rounds)
  {
    state.totalRounds = rounds
  },
  SET_DRAWER(state, drawer)
  {
    state.drawer = drawer
  },
  SET_TIME_REMAINING(state, time)
  {
    state.timeRemaining = time
  },
  SET_WORD(state, word)
  {
    state.word = word
  },
  ADD_DRAW_HISTORY(state, drawAction)
  {
    state.canvas.drawHistory.push(drawAction)
  },
  CLEAR_DRAW_HISTORY(state)
  {
    state.canvas.drawHistory = []
  },
  SET_CURRENT_DRAWING(state, drawing)
  {
    state.canvas.currentDrawing = drawing
  },
  ADD_GUESS(state, guess)
  {
    state.guesses.push(guess)
  },
  CLEAR_GUESSES(state)
  {
    state.guesses = []
  },
  ADD_ROUND_RESULT(state)
  {
    state.roundResults.push(result);
  },
  SET_GAME_RESULT(state, result)
  {
    state.gameResults.push(result)
  },
  RESET_GAME_STATE(state)
  {
    state.isGameStarted = false
    state.gameStatus = 'idle'
    state.currentRound = 0
    state.drawer = null
    state.timeRemaining = 0
    state.word = 
    {
      display: '',
      actual: '',
      hint: ''
    }
    state.canvas.drawHistory = []
    state.canvas.currentDrawing = null
    state.guesses = []
    state.roundREsults = []
    state.gameResults = null
  },
  SET_TIMER(state, timer)
  {
    state.timer = timer
  }
};

const actions = 
{
  // 开始游戏
  startGame({commit,dispatch,rootState},{roomSettings})
  {
    commit('SET_GAME_STARTED',true);
    commit('SET_GAME_STATUS','playing');
    commit('SET_TOTAL_ROUNDS',roomSettings?.rounds || 5);

    // 重置游戏状态
    commit('RESET_GAME_STATE');
    commit('SET_GAME_STARTED',true);

    // 启动第一回合
    dispatch('startNewRound');
  },
  // 开始新回合
  startNewRound({commit,state,roomState})
  {
    const newRound = state.currentRound + 1;
    commit('SET_CURRENT_ROUND',newRound);
    commit('SET_GAME_STATUS','playing');
    commit('CLEAR_GUESSES');
    commit('CLEAR_DRAW_HISTORY');

    // 选择绘画者
    const players = rootState.gameRoom.players;
    const drawerIndex = (newRound - 1) % players.length;
    const drawer = players[drawerIndex];
    commit('SET_DRAWER',drawer);

    // 设置单词（实际环境中应该从服务器获取）(这里应该要调用API接口)
    const words = ['苹果', '香蕉', '电脑', '篮球', '太阳', '月亮', '手机', '书本', '钢琴', '自行车'];
    const randomWord = words[Math.floor(Math.random() * words.length)];

    if(drawer.id === rootState.user.userInfo.id)
    {
      // 如果当前用户是绘画者，显示完整单词
      commit('SET_WORD',
        {
          display: randomWord,
          actual: randomWord,
          hint: `提示：${randomWord.length} 个字`
        }
      );
    }
    else 
    {
      // 如果是猜词者，显示占位符
      const placeholder = '_'.repeat(randomWord.length);
      commit('SET_WORD',
        {
          display: placeholder,
          actual: randomWord,
          hint: `提示:${random.length} 个字`
        }
      );
    }

    // 启动计时器
    const timer = setInterval(() => {
      if (state.timeRemaining > 0) {
        commit('SET_TIME_REMAINING', state.timeRemaining - 1);
      } else {
        clearInterval(state.timer);
        dispatch('endRound', { timedOut: true });
      }
    }, 1000);
    
    commit('SET_TIMER', timer);
  },
  // 提交绘画动作
  submitDrawing({commit,state,rootState},drawAction)
  {
    // 只有绘画者才能绘画
    if(state.drawer?.id !== rootState.user.userInfo.id) return;

    // 添加到绘画历史
    commit('ADD_DRAWER_HISTORY',drawAction);

    // 这里要通过sockitService发送到服务器
    // 等实现了再添加
  },
  // 提交猜测
  submitGuess({commit,state,dispatch,rootState},guessText)
  {
    // 绘画者不能猜测
    if(state.drawer?.id === rootState.user.userInfo.id) return;

    const userInfo = rootState.user.userInfo;
    const guess = 
    {
      userId: userInfo.id,
      userName: userInfo.userName,
      text: guessText,
      isCorrect: guessTExt == state.word.actual,
      timeStamp: new Date().toISOString()
    };

    // 添加到历史猜测记录
    commit('ADD_GUESS', guess);

    // 这里应该通过socketService发送到服务器

    // 如果猜对了
    if(guess.isCorrect)
    {
      // 结束当前回合
      dispatch('endRound',{correctGuess:true,guesser:userInfo});
    }
  },
  // 结束当前回合
  endRound({commit,state,dispatch},{timedOut,correctGuess,guesser})
  {
    clearInterval(state.timer);
    commit('SET_GAME_STATUS','roundEnd');

    // 计算本回合结果
    const roundResults = 
    {
      round: state.currentRound,
      word: state.word.actual,
      drawer: state.drawer,
      timedOut: timedOut || false,
      correctGuess: correctGuess || false,
      guesser: guesser || null,
      timeUsed: state.timeRemaining,
      scores:{}
    };

    if(correctGuess && guesser) 
    {
      // 计算得分：剩余时间越多，得分越高
      const guessScore = Math.max(10, Math.floor(state.timeRemaininig*2));
      const drawerScore = Math.floor(guessScore / 2);

      roundResults.scores[guesser.id] = guesserScore;
      roundResults.scores[state.drawer.id] = drawerScore;

      // 更新用户分数，通过调用user的action
      dispatch('user/updateScore',guesserScore,{root:true});
      // 绘画者的分数要通过WebSocket通过服务器处理
    }

    commit('ADD_ROUND_RESULT',roundREsult);

    // 判断游戏是否结束
    if(state.currentRound >= state.totalRounds)
    {
      dispatch('endGame');
    }
    else // 延迟几秒之后开始下一把
    {
      setTimeout (()=>
      {
        dispatch('startNewRound');
      },5000);
    }
  },
  // 结束游戏
  endGame({commit,state,rootState})
  {
    commit('SET_GAME_STATUS','gameEnd');

    // 计算游戏结果
    const players = rootState.gameRoom.players;
    const playerScores = {};

    // 初始化每个玩家的分数
    players.foreach(player => 
    {
      playerScores[player.id] = 0;
    }
    )

    // 累计每轮得分
    state.roundResults.foreach(
      result => {
        Object.keys(result.scores).forEach(playerId=>{
          playerScores[playerId] = (playerScores[playerId] || 0) + result.scores[playerId];
        })});
    
    // 转换为排名列表
    const rankings = players.map(player=>({
      id: player.id,
      username:player.username,
      avatar: player.avatar,
      score: playerScores[player.id] || 0
    })).sort((a,b)=>b.score-a.score);
    
    // 设置游戏结果
    commit('SET_GAME_RESULT',
      {
        rankings,
        rounds: state.roundResults
      }
    );

    // 更新用户游戏统计
    commit('user/UPDATE_GAME_STATS',{isWinner},{root:true});
  },
  // 重置游戏状态
  resetGame({commit})
  {
    clearInterval(state.timer);
    commit('RESET_GAME_STATE');
  }
};

const getters = 
{
  isGameActive: state => state.isGameStarted && state.gameStatus != 'gameEnd',
  isDrawer: (state, getters,rootState)=> 
  {
    return state.drawer?.id === rootState.user.userInfo.id;
  },
  currentWord: (state, getters) => 
  {
    return getters.isDrawer ? state.word.actual : state.word.display;
  },
  drawHistory: state => state.canvas.drawHistory,
  correctGuesses: state => state.guesses.filter(g=>g.isCorrect),
  isLastRound: state => state.currentRound === state.totalRounds,
  gameProgress: state => Math.floor((state.currentRound/state.totalRounds)*100),
  formattedTimeRemaining: state =>
  {
    const minutes = Math.floor(state.timeRemaining /60);
    const seconds = state.timeRemaining % 60;
    return `${minutes}:${seconds.toString().padStart(2,'0')}`;
  }
};

export default
{
  namespaced: true,
  state,
  mutations,
  actions,
  getters
};