<template>
    <div class="drawing-board-container">
        <!-- 上层工具栏 - 主要工具的选择-->
        <div class="main-tools">
            <button
                @click="selectTool('brush')"
                class="tool-btn"
                :class="{ active: currentTool === 'brush' }"
                title="画笔">
                <i class="fas fa-paint-brush"></i>
            </button>
            <button
                @click="selectTool('eraser')"
                class="tool-btn"
                :class="{ active: currentTool === 'eraser' }"
                title="橡皮檫">
                <i class="fas fa-eraser"></i>
            </button>
            <button
                @click="clearCanvas"
                class="tool-btn"
                title="清空画布">
                <i class="fas fa-trash"></i>
            </button>
            <button
                @click="undo"
                class="tool-btn"
                title="撤销"
                :disabled="!canUndo">
                <i class="fas fa-undo"></i>
            </button>
            <button
                @click="redo"
                class="tool-btn"
                title="重做"
                :disabled="!canRedo">
                <i class="fas fa-redo"></i>
            </button>
        </div>

        <!-- 下层工具栏 - 根据上层工具栏选择的工具给出对应的设置-->
        <div class="secondary-tools">
            <!--画笔设置-->
            <div v-if="currentTool === 'brush'" class="tool-settings">
                <div class="setting-group">
                    <label>颜色：</label>
                    <div class="color-picker">
                        <input type="color" v-model="currentColor" title="选择颜色">
                    </div>
                    <!--颜色预设-->
                    <div class="color-presets">
                        <button
                            v-for="color in colorPresets"
                            :key="color.value"
                            @click="currentColor = color.value"
                            class="color-preset-btn"
                            :style="{backgroundColor: color.value}"
                            :title="color.name">
                        </button>
                    </div>
                </div>

                <div class="setting-group">
                    <label>粗细：</label>
                    <div class="brush-size">
                        <input
                            type="range"
                            v-model="brushSize"
                            min="1"
                            max="20"
                            title="画笔大小">
                        <span class="size-label">{{ brushSize }}px</span>
                    </div>

                    <!--画笔预设-->
                    <div class="brush-presets">
                        <button
                            v-for="preset in brushPresets"
                            :key="preset.size"
                            @click="setBrushPreset(preset)"
                            class="preset-btn"
                            :class="{active:brushSize === preset.size}"
                            :title="preset.name">
                            <div
                                class="preset-circle"
                                :style="{width: preset.size + 'px', height: preset.size + 'px', backgroundColor: currentColor}">
                            </div>
                        </button>
                    </div>
                </div>
            </div>

            <!--橡皮檫设置-->
            <div v-if="currentTool === 'eraser'" class="tool-settings">
                <div class="setting-group">
                    <label>橡皮檫大小：</label>
                    <div class="brush-size">
                        <input
                            type="range"
                            v-model="eraserSize"
                            min="5"
                            max="50"
                            title="橡皮檫大小">
                        <span class="size-label">{{ eraserSize }}px</span>
                    </div>

                    <!--橡皮檫预设-->
                    <div class="brush-presets">
                        <button
                            v-for="preset in eraserPresets"
                            :key="preset.size"
                            @click="eraserSize = preset.size"
                            class="preset-btn"
                            :class="{active:eraserSize === preset.size}"
                            :title="preset.name">
                            <div
                                class="preset-circle eraser-preset"
                                :style="{width: preset.size + 'px', height: preset.size + 'px', backgroundColor: '#ffffff'}">
                            </div>
                        </button>
                    </div>
                </div>
            </div>
        </div>

        <!-- Canvas -->
        <div class="canvas-container" ref="canvasContainer">
            <canvas 
                ref="canvas"
                @mousedown="startDrawing"
                @mousemove="draw"
                @mouseup="stopDrawing"
                @mouseleave="stopDrawing"
                @touchstart="handleTouchStart"
                @touchmove="handleTouchMove"
                @touchend="stopDrawing"
            ></canvas>

            <!--自定义光标-->
            <div
                v-if="showCustomCursor"
                class="custom-cursor"
                :class="{'eraser-cursor': currentTool === 'eraser'}"
                :style="{
                    width:(currentTool === 'brush'? brushSize:eraserSize) + 'px',
                    height:(currentTool === 'brush'? brushSize:eraserSize) + 'px',
                    backgroundColor: currentTool === 'brush' ? currentColor : '#ffffff',
                    border: currentTool === 'eraser' ? '1px dashed #666' : 'none',
                }">
            </div>

            <div v-if="isDrawing" class="drawing-indicator">
                {{ currentTool === 'brush'? '正在绘画...' : '正在擦除...' }}
            </div>
        </div>
    </div>
</template>

<script>
import signalRService from '@/services/signalRService.js'
export default {
    name: 'DrawingBoard',
    props:
    {
        readonly:
        {
            type: Boolean,
            default: false
        },
        remoteStrokes:
        {
            type: Array,
            default: () => []
        }
    },
    // watch:{
    //     remoteStrokes:{
    //         handler(newStrokes){

    //             // 调试信息
    //             console.log('在DrawingBoard.vue的remoteStrokes监听器中接收到远程笔画数据改变');
                
    //             this.strokes = [...newStrokes];
    //             this.redrawCanvas();
    //         },
    //         deep:true
    //     }
    // },
    data() {
        return {
            isDrawing: false,
            currentTool: 'brush', //当前选择的工具
            currentColor: '#000000',
            brushSize: 5,
            eraserSize: 20,
            context: null,
            canvas: null,
            currentX: 0,
            currentY: 0,
            strokes: [], // 存储所有笔画
            currentStroke: [], // 存储当前笔画
            undoneStrokes: [], // 存储撤销的笔画
            brushPresets: [
                { size: 2, name: '细笔', icon: 'fas fa-pen' },
                { size: 5, name: '中笔', icon: 'fas fa-paint-brush' },
                { size: 10, name: '粗笔', icon: 'fas fa-marker' },
                { size: 15, name: '特粗笔', icon: 'fas fa-highlighter' }
            ],
            eraserPresets:[
                {size:10, name:'小橡皮'},
                {size:20, name:'中橡皮'},
                {size:30, name:'大橡皮'},
                {size:40, name:'特大橡皮'}
            ],
            lastDrawTime: 0,
            drawThrottle: 16, // 约60fps
            // 添加颜色预设
            colorPresets:[
                {name: '红色', value: '#FF0000'},
                {name: '绿色', value: '#00FF00'},
                {name: '蓝色', value: '#0000FF'},
                {name: '黄色', value: '#FFFF00'},
                {name: '紫色', value: '#800080'},
                {name: '橙色', value: '#FFA500'},
                {name: '粉色', value: '#FFC0CB'},
                {name: '黑色', value: '#000000'}
            ],
            showCustomCursor: false, //显示自定义光标
            mousePosition: { x: 0, y: 0 }, // 鼠标位置
        }
    },
    computed: {
        canUndo() {
            return this.strokes.length > 0
        },
        canRedo() {
            return this.undoneStrokes.length > 0
        }
    },
    watch:{
        remoteStrokes:{
            handler(newStrokes){
                this.strokes = [...newStrokes];
                this.redrawCanvas();
            },
            deep: true
        }
    },
    mounted() {
        this.initializeCanvas()
        window.addEventListener('resize', this.resizeCanvas)

        // 设置自定义光标
        const canvasEl = this.$refs.canvas
        if(canvasEl){
            canvasEl.addEventListener('mousemove', this.updateCursorPosition)
            canvasEl.addEventListener('mouseenter', ()=>{this.showCustomCursor = true})
            canvasEl.addEventListener('mouseleave', ()=>{this.showCustomCursor = false})
        } 

        // 注册接收远程笔画的回调
        signalRService.registerStrokeReceivedCallback((strokeData) => {
            console.log('在DrawingBoard.vue的mounted函数中接收到远程笔画数据', strokeData);
            this.strokes.push(strokeData);
            this.redrawCanvas();
        })

        // 注册接收撤销操作的回调
        signalRService.registerUndoReceivedCallback(() => {
            console.log('在DrawingBoard.vue的mounted函数中接收到撤销操作');
            if(this.readonly) return;
            if(this.strokes.length === 0) return;
            const lastStroke = this.strokes.pop();
            this.undoneStrokes.push(lastStroke);
            this.redrawCanvas();
            console.log('如果无法撤销，那就是这里有问题');
        })

        // 注册接收重做操作的回调
        signalRService.registerRedoReceivedCallback(() => {
            if(this.readonly) return;
            if(this.undoneStrokes.length === 0) return;
            const stroke = this.undoneStrokes.pop();
            this.strokes.push(stroke);
            this.redrawCanvas();
        })

        // 注册接收清空画布的回调
        signalRService.registerClearReceivedCallback(() => {
            if(this.readonly) return;
            this.context.clearRect(0, 0, this.canvas.width, this.canvas.height);
            this.strokes = [];
            this.undoneStrokes = [];
        })
    },
    beforeDestroy() {
        window.removeEventListener('resize', this.resizeCanvas)

        const canvasEl = this.$refs.canvas
        if(canvasEl){
            canvasEl.removeEventListener('mousemove', this.updateCursorPosition)
            canvasEl.removeEventListener('mouseenter', ()=>{this.showCustomCursor = true})
            canvasEl.removeEventListener('mouseleave', ()=>{this.showCustomCursor = false})
        }
    },
    methods: {
        initializeCanvas() {
            this.canvas = this.$refs.canvas
            this.context = this.canvas.getContext('2d')
            this.resizeCanvas()
            
            // 设置画布样式
            this.context.lineCap = 'round'
            this.context.lineJoin = 'round'
            this.context.imageSmoothingEnabled = true
            this.context.imageSmoothingQuality = 'high'
        },

        resizeCanvas() {
            const container = this.$refs.canvasContainer
            this.canvas.width = container.clientWidth
            this.canvas.height = container.clientHeight
        },

        selectTool(tool)
        {
            this.currentTool = tool
        },

        updateCursorPosition(event)
        {
            const rect = this.canvas.getBoundingClientRect()
            this.mousePosition = 
            {
                x: event.clientX - rect.left,
                y: event.clientY - rect.top
            }
        },

        startDrawing(event) {
            if(this.readonly) return

            this.isDrawing = true
            const { x, y } = this.getCoordinates(event)
            this.currentX = x
            this.currentY = y
            
            if(this.currentTool === 'brush'){
                this.currentStroke = [{
                    x,y,
                    color: this.currentColor,
                    size: this.brushSize,
                    tool:'brush'
                }]
            }
            else if(this.currentTool === 'eraser'){
                this.currentStroke = [{
                    x,y,
                    size: this.eraserSize,
                    tool:'eraser'
                }]
            }

            this.lastDrawTime = Date.now()
        },

        draw(event) {
            if (!this.isDrawing || this.readonly) return

            const now = Date.now()
            if (now - this.lastDrawTime < this.drawThrottle) return
            this.lastDrawTime = now

            const { x, y } = this.getCoordinates(event)
            
            this.context.beginPath()
            this.context.moveTo(this.currentX, this.currentY)
            this.context.lineTo(x, y)

            if(this.currentTool === 'brush'){
                this.context.strokeStyle = this.currentColor
                this.context.lineWidth = this.brushSize
                this.context.stroke()
                this.currentStroke.push({
                    x,y,
                    color: this.currentColor,
                    size: this.brushSize,
                    tool: 'brush'
                })
            }
            else if(this.currentTool === 'eraser'){
                // 使用destination-out合成操作擦除
                this.context.globalCompositeOperation = 'destination-out'
                this.context.lineWidth = this.eraserSize
                this.context.stroke()
                this.context.globalCompositeOperation = 'source-over' // 恢复默认合成操作
                this.currentStroke.push({
                    x,y,
                    size: this.eraserSize,
                    tool: 'eraser'
                })
            }
            
            this.currentX = x
            this.currentY = y
        },

        async stopDrawing() {
            if (this.isDrawing) {
                this.isDrawing = false
                if (this.currentStroke.length > 0) {
                    this.strokes.push([...this.currentStroke])
                    this.undoneStrokes = [] // 清空重做栈
                    // 重置当前笔画
                    this.currentStroke = []

                    // 调试信息
                    //console.log('在DrawingBoard.vue的stopDrawing函数中发送笔画数据到GameRoom.vue', this.strokes[this.strokes.length-1])

                    //this.$emit('stroke-completed', this.strokes[this.strokes.length - 1]) //会将这一段笔画发送到GameRoom.vue中
                    await signalRService.sendStroke(this.strokes[this.strokes.length-1])

                    // 调试信息
                    console.log('在DrawingBoard.vue的stopDrawing函数中发送笔画数据到SignalR服务', this.strokes[this.strokes.length-1])
                }
            }
        },

        getCoordinates(event) {
            const rect = this.canvas.getBoundingClientRect()
            let x, y
            
            if (event.touches) {
                x = event.touches[0].clientX - rect.left
                y = event.touches[0].clientY - rect.top
            } else {
                x = event.clientX - rect.left
                y = event.clientY - rect.top
            }
            
            return { x, y }
        },

        handleTouchStart(event) {
            if(this.readonly) return
            event.preventDefault()
            this.startDrawing(event)
        },

        handleTouchMove(event) {
            if(this.readonly) return
            event.preventDefault()
            this.draw(event)
        },

        clearCanvas() {
            if(this.readonly) return
            this.context.clearRect(0, 0, this.canvas.width, this.canvas.height)
            this.strokes = []
            this.undoneStrokes = []
            this.$emit('canvas-cleared')
            // 同步到后端
            signalRService.sendClear(1); //传入房间号
        },

        undo() {
            if (this.strokes.length === 0 || this.readonly) return
            
            const lastStroke = this.strokes.pop()
            this.undoneStrokes.push(lastStroke)
            this.redrawCanvas()
            this.$emit('stroke-undone')
            // 同步到后端
            signalRService.sendUndo(1); //传入房间号
        },

        redo() {
            if (this.undoneStrokes.length === 0 || this.readonly) return
            
            const stroke = this.undoneStrokes.pop()
            this.strokes.push(stroke)
            this.redrawCanvas()
            this.$emit('stroke-redone')
            // 同步到后端
            signalRService.sendRedo(1); //传入房间号
        },

        setBrushPreset(preset) {
            this.brushSize = preset.size
        },

        redrawCanvas() {
            this.context.clearRect(0, 0, this.canvas.width, this.canvas.height)
            
            this.strokes.forEach(stroke => {
                if(stroke.length < 2) return;
                const toolType = stroke[0].tool;
                for (let i = 1; i < stroke.length; i++) {
                    this.context.beginPath()
                    this.context.moveTo(stroke[i-1].x, stroke[i-1].y)
                    this.context.lineTo(stroke[i].x, stroke[i].y)
                    if(toolType === 'brush'){
                        this.context.globalCompositeOperation = 'source-over'
                        this.context.strokeStyle = stroke[i].color
                        this.context.lineWidth = stroke[i].size
                    }
                    else if(toolType === 'eraser'){
                        this.context.globalCompositeOperation = 'destination-out'
                        this.context.lineWidth = stroke[i].size
                    }
                    this.context.stroke()
                }
                this.context.globalCompositeOperation = 'source-over'
            })
        }
    }
}
</script>



<style scoped>
.drawing-board-container {
    width: 100%;
    height: 100%;
    display: flex;
    flex-direction: column;
    background: #ffffff;
    border-radius: 8px;
    overflow: hidden;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

/* 上层工具栏 */
.main-tools{
    display:flex;
    gap:10px;
    padding:12px;
    background: #f0f3f5;
    align-items: center;
    justify-content: center;
    border-bottom: 1px solid #e0e4e8;
}

/* 下层次要工具栏 */
.secondary-tools{
    background: #f8fafc;
    border-bottom: 1px solid #e0e4e8;
    padding: 10px;
}

.tool-settings{
    display:flex;
    flex-wrap: wrap;
    gap: 20px;
}

.setting-group label{
    font-size: 14px;
    color: #5a6474;
    font-weight: 500;
    width: 60px;
}

.drawing-tools {
    display: flex;
    gap: 15px;
    padding: 15px;
    background: #f8f9fa;
    align-items: center;
    border-bottom: 1px solid #e9ecef;
}

.tool-group {
    display: flex;
    gap: 8px;
    align-items: center;
    padding: 0 10px;
    border-right: 1px solid #e9ecef;
}

.tool-group:last-child {
    border-right: none;
}

.canvas-container {
    flex: 1;
    position: relative;
    overflow: hidden;
}

canvas {
    width: 100%;
    height: 100%;
    cursor: crosshair;
    touch-action: none;
}

/* 自定义光标 */
.custom-cursor{
    position: absolute;
    pointer-events: none;
    border-radius: 50%;
    transform: translate(-50%, -50%);
    z-index: 10;
}

.eraser-cursor{
    background-color: rgba(255, 255, 255, 0.5) !important;
    border: 1px dashed #666;
}

.tool-btn {
    padding: 10px;
    border: none;
    border-radius: 6px;
    background: #ffffff;
    color: #495057;
    cursor: pointer;
    transition: all 0.2s ease;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 16px;
}

.tool-btn:hover:not(:disabled) {
    background: #e9ecef;
    color: #212529;
}

.tool-btn:disabled {
    opacity: 0.5;
    cursor: not-allowed;
}

.tool-btn.active {
    background: #e9ecef;
    color: #212529;
}

/* 颜色选择器 */
.color-picker input {
    width: 32px;
    height: 32px;
    padding: 0;
    border: none;
    border-radius: 4px;
    cursor: pointer;
}

.brush-size {
    display: flex;
    align-items: center;
    gap: 8px;
}

.brush-size input {
    width: 100px;
}

.size-label {
    font-size: 12px;
    color: #6c757d;
    min-width: 40px;
}

.drawing-indicator {
    position: absolute;
    top: 10px;
    right: 10px;
    background: rgba(0, 0, 0, 0.7);
    color: white;
    padding: 5px 10px;
    border-radius: 4px;
    font-size: 12px;
    animation: fadeIn 0.3s ease;
}

.color-presets{
    display:flex;
    flex-wrap:wrap;
    gap:5px;
    margin:0 5px;
}

.color-preset-btn{
    width: 20px;
    height: 20px;
    border: 1px solid #e9ecef;
    border-radius: 50%;
    cursor: pointer;
    transition: transform 0.2s ease;
}

.color-preset-btn:hover {
    transform: scale(1.2);
    border:1px solid #abd5bd;
}

.brush-size{
    display: flex;
    align-items: center;
    gap: 8px;
}

.brush-size input{
    width: 120px;
}

.size-label{
    font-size: 12px;
    color: #6c757d;
    min-width: 40px;
    text-align: center;
}

/* 画笔预设 */
.brush-presets{
    display: flex;
    gap: 6px;
}

.preset-btn{
    background: transparent;
    border: 1px solid #dee2e6;
    border-radius: 4px;
    padding: 4px;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
}

.preset-btn.active{
    border-color: #7048e8;
    background-color: #f3f0ff;
}

.preset-circle {
    background-color: #000;
    border-radius: 50%;
    min-width: 2px;
    min-height: 2px;
}

.eraser-preset {
    background-color: #f8f9fa;
    border: 1px dashed #adb5bd;
}

.drawing-indicator {
    position: absolute;
    top: 10px;
    right: 10px;
    background: rgba(0, 0, 0, 0.7);
    color: white;
    padding: 5px 10px;
    border-radius: 4px;
    font-size: 12px;
    animation: fadeIn 0.3s ease;
}

@keyframes fadeIn {
    from { opacity: 0; }
    to { opacity: 1; }
}

@media (max-width: 768px) {
    .main-tools, .tool-settings {
        flex-wrap: wrap;
        gap: 8px;
    }
    
    .setting-group {
        width: 100%;
        justify-content: center;
        padding: 5px 0;
    }
    
    .setting-group label {
        width: auto;
        min-width: 60px;
    }
    
    .brush-size input {
        width: 100px;
    }
    
    .tool-btn {
        padding: 8px;
        font-size: 14px;
    }
}
</style>
