<template>
    <div class="chat-container">
        <div class="chat-messages" ref="messageContainer">
            <div v-for="(message, index) in messages" :key="index" class="message">
                <span class="username" :style="{ color: message.isSystem ? '#ff6b6b' : '#4a90e2' }">
                    {{ message.username }}:
                </span>
                <span class="content" :class="{ 'system-message': message.isSystem }">
                    {{ message.content }}
                </span>
            </div>
        </div>
        <div class="chat-input">
            <input
                v-model="newMessage"
                @keyup.enter="sendMessage"
                :placeholder="isDrawer ? '你是画师，不能发送消息' : '输入你的猜测...'"
                :disabled="isDrawer"
            />
            <button @click="sendMessage" :disabled="isDrawer">发送</button>
        </div>
    </div>
</template>

<script>
export default {
    name: 'Chat',
    props: {
        isDrawer: {
            type: Boolean,
            default: false
        }
    },
    data() {
        return {
            messages: [],
            newMessage: '',
        }
    },
    methods: {
        sendMessage() {
            if (this.isDrawer || !this.newMessage.trim()) return;

            // Emit the message to parent component
            this.$emit('send-message', {
                content: this.newMessage,
                username: this.$store.state.user.username,
                timestamp: new Date().toISOString()
            });

            this.newMessage = '';
        },
        addMessage(message) {
            this.messages.push(message);
            this.$nextTick(() => {
                this.scrollToBottom();
            });
        },
        scrollToBottom() {
            const container = this.$refs.messageContainer;
            container.scrollTop = container.scrollHeight;
        }
    }
}
</script>

<style scoped>
.chat-container {
    display: flex;
    flex-direction: column;
    height: 100%;
    border: 1px solid #ddd;
    border-radius: 4px;
}

.chat-messages {
    flex-grow: 1;
    overflow-y: auto;
    padding: 10px;
    background-color: #f8f9fa;
    min-height: 300px;
    max-height: 400px;
}

.message {
    margin-bottom: 8px;
    line-height: 1.4;
}

.username {
    font-weight: bold;
    margin-right: 5px;
}

.content {
    word-break: break-word;
}

.system-message {
    color: #ff6b6b;
    font-style: italic;
}

.chat-input {
    display: flex;
    padding: 10px;
    background-color: #fff;
    border-top: 1px solid #ddd;
}

.chat-input input {
    flex-grow: 1;
    padding: 8px;
    border: 1px solid #ddd;
    border-radius: 4px;
    margin-right: 8px;
}

.chat-input button {
    padding: 8px 16px;
    background-color: #4a90e2;
    color: white;
    border: none;
    border-radius: 4px;
    cursor: pointer;
}

.chat-input button:disabled {
    background-color: #ccc;
    cursor: not-allowed;
}

.chat-input input:disabled {
    background-color: #f5f5f5;
    cursor: not-allowed;
}
</style>