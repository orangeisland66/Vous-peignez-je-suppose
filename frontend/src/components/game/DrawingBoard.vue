<template>
    <div class="drawing-board-container">
        <!-- Drawing Tools -->
        <div class="drawing-tools">
            <div class="tool-group">
                <div class="color-picker">
                    <input type="color" v-model="currentColor" title="选择颜色">
                </div>
                <!--增加颜色预设功能-->
                <div class = "color-presets">
                    <button
                        v-for="color in colorPresets"
                        :key="color.value"
                        @click="currentColor = color.value"
                        class="color-preset-btn"
                        :style="{backgroundColor: color.value}"
                        :title="color.name"
                        ></button>
                </div>
                <div class="brush-size">
                    <input 
                        type="range" 
                        v-model="brushSize" 
                        min="1" 
                        max="20" 
                        title="画笔大小"
                    >
                    <span class="size-label">{{ brushSize }}px</span>
                </div>
            </div>
            
            <div class="tool-group">
                <button @click="clearCanvas" class="tool-btn" title="清空画布">
                    <i class="fas fa-trash"></i>
                </button>
                <button @click="undo" class="tool-btn" title="撤销" :disabled="!canUndo">
                    <i class="fas fa-undo"></i>
                </button>
                <button @click="redo" class="tool-btn" title="重做" :disabled="!canRedo">
                    <i class="fas fa-redo"></i>
                </button>
            </div>

            <div class="tool-group">
                <button 
                    v-for="preset in brushPresets" 
                    :key="preset.size"
                    @click="setBrushPreset(preset)"
                    class="tool-btn"
                    :class="{ active: brushSize === preset.size }"
                    :title="preset.name"
                >
                    <i :class="preset.icon"></i>
                </button>
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
            <div v-if="isDrawing" class="drawing-indicator">
                正在绘画...
            </div>
        </div>
    </div>
</template>

<script>
export default {
    name: 'DrawingBoard',
    data() {
        return {
            isDrawing: false,
            currentColor: '#000000',
            brushSize: 5,
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
            ]
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
    mounted() {
        this.initializeCanvas()
        window.addEventListener('resize', this.resizeCanvas)
    },
    beforeDestroy() {
        window.removeEventListener('resize', this.resizeCanvas)
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

        startDrawing(event) {
            this.isDrawing = true
            const { x, y } = this.getCoordinates(event)
            this.currentX = x
            this.currentY = y
            this.currentStroke = [{ x, y, color: this.currentColor, size: this.brushSize }]
            this.lastDrawTime = Date.now()
        },

        draw(event) {
            if (!this.isDrawing) return

            const now = Date.now()
            if (now - this.lastDrawTime < this.drawThrottle) return
            this.lastDrawTime = now

            const { x, y } = this.getCoordinates(event)
            
            this.context.beginPath()
            this.context.moveTo(this.currentX, this.currentY)
            this.context.lineTo(x, y)
            this.context.strokeStyle = this.currentColor
            this.context.lineWidth = this.brushSize
            this.context.stroke()

            this.currentX = x
            this.currentY = y
            this.currentStroke.push({ x, y, color: this.currentColor, size: this.brushSize })
        },

        stopDrawing() {
            if (this.isDrawing) {
                this.isDrawing = false
                if (this.currentStroke.length > 0) {
                    this.strokes.push([...this.currentStroke])
                    this.undoneStrokes = [] // 清空重做栈
                    this.currentStroke = []
                    this.$emit('stroke-completed', this.strokes[this.strokes.length - 1])
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
            event.preventDefault()
            this.startDrawing(event)
        },

        handleTouchMove(event) {
            event.preventDefault()
            this.draw(event)
        },

        clearCanvas() {
            this.context.clearRect(0, 0, this.canvas.width, this.canvas.height)
            this.strokes = []
            this.undoneStrokes = []
            this.$emit('canvas-cleared')
        },

        undo() {
            if (this.strokes.length === 0) return
            
            const lastStroke = this.strokes.pop()
            this.undoneStrokes.push(lastStroke)
            this.redrawCanvas()
            this.$emit('stroke-undone')
        },

        redo() {
            if (this.undoneStrokes.length === 0) return
            
            const stroke = this.undoneStrokes.pop()
            this.strokes.push(stroke)
            this.redrawCanvas()
            this.$emit('stroke-redone')
        },

        setBrushPreset(preset) {
            this.brushSize = preset.size
        },

        redrawCanvas() {
            this.context.clearRect(0, 0, this.canvas.width, this.canvas.height)
            
            this.strokes.forEach(stroke => {
                for (let i = 1; i < stroke.length; i++) {
                    this.context.beginPath()
                    this.context.moveTo(stroke[i-1].x, stroke[i-1].y)
                    this.context.lineTo(stroke[i].x, stroke[i].y)
                    this.context.strokeStyle = stroke[i].color
                    this.context.lineWidth = stroke[i].size
                    this.context.stroke()
                }
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

.tool-btn {
    padding: 8px;
    border: none;
    border-radius: 4px;
    background: #ffffff;
    color: #495057;
    cursor: pointer;
    transition: all 0.2s ease;
    display: flex;
    align-items: center;
    justify-content: center;
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

.color-picker input {
    width: 40px;
    height: 40px;
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
    margin:0 10px;
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

@keyframes fadeIn {
    from { opacity: 0; }
    to { opacity: 1; }
}

@media (max-width: 768px) {
    .drawing-tools {
        flex-wrap: wrap;
        gap: 10px;
    }

    .tool-group {
        padding: 5px;
        border-right: none;
    }

    .brush-size input {
        width: 80px;
    }

    .color-presets{
        margin: 5px 0;
    }
}
</style>

