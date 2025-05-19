<template>
    <div class="chat-container">
        <div class="chat-header">
            <!-- <h3>游戏聊天</h3> -->
            
        </div>

        <div class="chat-messages" ref="messageContainer">
            <div v-if="messages.length === 0" class="empty-state">
                <i class="fas fa-comments"></i>
                <p>暂无消息</p>
            </div>
            <div v-else>
                <div 
                    v-for="(message, index) in messages" 
                    :key="index" 
                    class="message"
                    :class="{
                        'system-message': message.isSystem,
                        'guess-correct': message.isCorrect,
                        'guess-wrong': message.isWrong
                    }"
                >
                    <div class="message-header">
                        <span class="username" :style="{ color: getUsernameColor(message) }">
                            {{ message.username }}
                        </span>
                        <span class="timestamp">{{ formatTime(message.timestamp) }}</span>
                    </div>
                    <div class="message-content">
                        <span class="content">{{ message.content }}</span>
                        <div v-if="message.isSystem" class="system-icon">
                            <i class="fas fa-info-circle"></i>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <div class="emoji-picker" v-if="showEmojiPicker">
            <div class="emoji-grid">
                <span 
                    v-for="emoji in emojis" 
                    :key="emoji"
                    @click="insertEmoji(emoji)"
                    class="emoji"
                >
                    {{ emoji }}
                </span>
            </div>
        </div>
     
        <div class="chat-input">
            <div class="chat-actions">
                <button 
                    class="action-btn" 
                    @click.stop="toggleEmojiPicker"
                    title="表情"
                >
                    <i class="fas fa-smile"></i>
                </button>
                <!-- <button 
                    class="action-btn" 
                    @click.stop="clearMessages"
                    title="清空消息"
                >
                    <i class="fas fa-trash"></i>
                </button> -->
            </div>
            <div class="input-wrapper">
                <input
                    v-model="newMessage"
                    @keyup.enter="sendMessage"
                    @keyup.esc="clearInput"
                    :placeholder="getInputPlaceholder"
                    :disabled="isDrawer"
                    ref="messageInput"
                />
                <div class="input-actions">
                    <span class="char-count" :class="{ 'warning': isCharCountWarning }">
                        {{ newMessage.length }}/50
                    </span>
                </div>
            </div>
            <button 
                @click="sendMessage" 
                :disabled="isDrawer || !canSendMessage || isSending"
                class="send-btn"
            >
                <i class="fas fa-paper-plane"></i>
            </button>
        </div>
    </div>
</template>

<script setup>
import { ref, computed, onMounted, onBeforeUnmount } from 'vue';
import { useRoute } from 'vue-router';
import signalRService from '../../services/signalRService'; 

// 接收父组件传递的 props
const props = defineProps({
    isDrawer: {
        type: Boolean,
        default: false
    },
    currentWord: {
        type: String,
        default: ''
    }
});

const messageInput = ref(null);
const messageContainer = ref(null);
const newMessage = ref('');
const messages = signalRService.chatMessages;
const showEmojiPicker = ref(false);
const emojis = ['😊', '😂', '🎨', '🎯', '🎮', '🏆', '👏', '💪', '🤔', '🎲'];
const maxMessageLength = 50;
const lastMessageTime = ref(0);
// const messageCooldown = 10; // 0.1秒冷却时间
const isSending = ref(false);

// 计算属性：判断是否可以发送消息
const canSendMessage = computed(() => {
    return newMessage.value.trim().length > 0 && 
           newMessage.value.length <= maxMessageLength 
});

// 计算属性：判断字符数是否达到警告阈值
const isCharCountWarning = computed(() => {
    return newMessage.value.length > maxMessageLength * 0.8;
});

// 计算属性：获取输入框占位符
const getInputPlaceholder = computed(() => {
    if (props.isDrawer) {
        return '你是画师，不能发送消息';
    }
    return props.currentWord? '输入你的猜测...' : '发送消息...';
});

// 发送消息方法
const sendMessage = async () => {
  if (!canSendMessage.value) return;

  try {
    const message = newMessage.value.trim();
    console.log('【前端】发送消息:', message); // 打印发送的消息内容
    if (!canSendMessage.value || isSending.value) return; // 新增：检查发送状态
  
    isSending.value = true; // 开始发送，禁用按钮
    await signalRService.sendChatMessage(message);
    newMessage.value = '';
    lastMessageTime.value = Date.now();
  } catch (error) {
    console.error('【前端】发送消息失败:', error);
  } finally {
    isSending.value = false; // 恢复按钮
  }
};

// 添加消息到消息列表并滚动到底部
const addMessage = (message) => {
    messages.value.push(message);
    const container = messageContainer.value;
    container.scrollTop = container.scrollHeight;
};

// 滚动到消息列表底部
const scrollToBottom = () => {
    const container = messageContainer.value;
    container.scrollTop = container.scrollHeight;
};

// 清空消息列表
const clearMessages = () => {
    messages.value = [];
};

// 清空输入框内容
const clearInput = () => {
    newMessage.value = '';
    showEmojiPicker.value = false;
};

// 切换表情选择器显示状态
const toggleEmojiPicker = () => {
    showEmojiPicker.value = !showEmojiPicker.value;
    console.log('表情选择器状态:', showEmojiPicker.value);
};

// 插入表情到输入框
const insertEmoji = (emoji) => {
    if (newMessage.value.length + emoji.length <= maxMessageLength) {
        newMessage.value += emoji;
    }
};

// 格式化时间
const formatTime = (timestamp) => {
    const date = new Date(timestamp);
    return date.toLocaleTimeString('zh-CN', { 
        hour: '2-digit', 
        minute: '2-digit',
        second: '2-digit'
    });
};

// 获取用户名颜色
const getUsernameColor = (message) => {
    if (message.isSystem) return '#ff6b6b';
    if (message.isCorrect) return '#28a745';
    return '#4a90e2';
};

const route = useRoute();

// 点击外部关闭表情选择器
const closeEmojiPicker = (e) => {
    if (!messageContainer.value.contains(e.target)) {
        showEmojiPicker.value = false;
    }
};

onMounted(async () => {
    document.addEventListener('click', closeEmojiPicker);
    // 调用 initialize 方法
    await signalRService.initialize(route.params.roomId);
});

onBeforeUnmount(() => {
    document.removeEventListener('click', closeEmojiPicker);
});
</script>

<style scoped>
.chat-container {
    display:flex;
    flex-direction: column;
    height: 100%;
    background: #ffffff;
    border-radius: 8px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    overflow: hidden;
}

.chat-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    /* padding: 12px 16px; */
    background: #f8f9fa;
    border-bottom: 1px solid #e9ecef;
}

.chat-header h3 {
    margin: 0;
    font-size: 16px;
    color: #212529;
}

.chat-actions {
    display: flex;
    float: left;
    /* gap: 8px; */
}

.action-btn {
    padding: 6px;
    border: none;
    background: transparent;
    color: #6c757d;
    cursor: pointer;
    border-radius: 4px;
    transition: all 0.2s ease;
}

.action-btn:hover {
    background: #e9ecef;
    color: #212529;
}

.chat-messages {
    flex-grow: 1;
    overflow-y: auto;
    padding: 16px;
    background-color: #ffffff;
}

.empty-state {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    height: 100%;
    color: #adb5bd;
}

.empty-state i {
    font-size: 48px;
    margin-bottom: 16px;
}

.message {
    margin-bottom: 16px;
    animation: fadeIn 0.3s ease;
}

.message-header {
    display: flex;
    align-items: center;
    gap: 8px;
    margin-bottom: 4px;
}

.username {
    font-weight: 600;
}

.timestamp {
    font-size: 12px;
    color: #adb5bd;
}

.message-content {
    display: flex;
    align-items: flex-start;
    gap: 8px;
}

.content {
    word-break: break-word;
    line-height: 1.4;
}

.system-message {
    background: #fff3cd;
    padding: 8px;
    border-radius: 4px;
    margin: 8px 0;
}

.guess-correct {
    background: #d4edda;
    padding: 8px;
    border-radius: 4px;
}

.guess-wrong {
    background: #f8d7da;
    padding: 8px;
    border-radius: 4px;
}

.system-icon {
    color: #ff6b6b;
}

.emoji-picker {
    position:re;
    background: white;
    border: 1px solid #e9ecef;
    border-radius: 4px;
    padding: 8px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    z-index: 1000;
}

.emoji-grid {
    display: grid;
    grid-template-columns: repeat(5, 1fr);
    gap: 8px;
}

.emoji {
    font-size: 24px;
    cursor: pointer;
    text-align: center;
    padding: 4px;
    border-radius: 4px;
    transition: background-color 0.2s ease;
}

.emoji:hover {
    background-color: #e9ecef;
}

.chat-input {
    display: flex;
    gap: 8px;
    padding: 16px;
    background-color: #f8f9fa;
    border-top: 1px solid #e9ecef;
    position: relative;
}

.input-wrapper {
    flex-grow: 1;
    position: relative;
}

.chat-input input {
    width: 100%;
    padding: 8px 12px;
    border: 1px solid #ced4da;
    border-radius: 4px;
    font-size: 14px;
    transition: border-color 0.2s ease;
}

.chat-input input:focus {
    outline: none;
    border-color: #4a90e2;
}

.chat-input input:disabled {
    background-color: #e9ecef;
    cursor: not-allowed;
}

.input-actions {
    position: absolute;
    right: 8px;
    top: 50%;
    transform: translateY(-50%);
    display: flex;
    align-items: center;
    gap: 8px;
}

.char-count {
    font-size: 12px;
    color: #6c757d;
}

.char-count.warning {
    color: #dc3545;
}

.send-btn {
    padding: 8px 16px;
    background-color: #4a90e2;
    color: white;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    transition: background-color 0.2s ease;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 14px;
}

.send-btn:hover:not(:disabled) {
    background-color: #357abd;
}

.send-btn:disabled {
    background-color: #adb5bd;
    cursor: not-allowed;
}

@keyframes fadeIn {
    from { opacity: 0; transform: translateY(10px); }
    to { opacity: 1; transform: translateY(0); }
}

@media (max-width: 768px) {
    .chat-header {
        padding: 8px 12px;
    }

    .chat-messages {
        padding: 12px;
    }

    .chat-input {
        padding: 12px;
    }

    .emoji-grid {
        grid-template-columns: repeat(4, 1fr);
    }
}
</style>