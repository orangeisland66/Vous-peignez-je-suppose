<template>
    <div class="drawing-board-container">
        <!-- Drawing Tools -->
        <div class="drawing-tools">
            <div class="color-picker">
                <input type="color" v-model="currentColor" title="Choose color">
            </div>
            <div class="brush-size">
                <input 
                    type="range" 
                    v-model="brushSize" 
                    min="1" 
                    max="20" 
                    title="Brush size"
                >
            </div>
            <button @click="clearCanvas" class="tool-btn">Clear</button>
            <button @click="undo" class="tool-btn">Undo</button>
        </div>

        <!-- Canvas -->
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
            strokes: [], // Store all strokes for undo functionality
            currentStroke: [] // Store current stroke points
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
            
            // Set initial canvas style
            this.context.lineCap = 'round'
            this.context.lineJoin = 'round'
        },

        resizeCanvas() {
            const container = this.canvas.parentElement
            this.canvas.width = container.clientWidth
            this.canvas.height = container.clientHeight - 50 // Subtract toolbar height
        },

        startDrawing(event) {
            this.isDrawing = true
            const { x, y } = this.getCoordinates(event)
            this.currentX = x
            this.currentY = y
            this.currentStroke = [{ x, y, color: this.currentColor, size: this.brushSize }]
        },

        draw(event) {
            if (!this.isDrawing) return

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
                    this.currentStroke = []
                    this.$emit('stroke-completed', this.strokes[this.strokes.length - 1])
                }
            }
        },

        getCoordinates(event) {
            const rect = this.canvas.getBoundingClientRect()
            let x, y
            
            if (event.touches) { // Touch event
                x = event.touches[0].clientX - rect.left
                y = event.touches[0].clientY - rect.top
            } else { // Mouse event
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
            this.$emit('canvas-cleared')
        },

        undo() {
            if (this.strokes.length === 0) return
            
            this.strokes.pop()
            this.redrawCanvas()
            this.$emit('stroke-undone')
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
}

.drawing-tools {
    display: flex;
    gap: 10px;
    padding: 10px;
    background: #f5f5f5;
    align-items: center;
}

canvas {
    border: 1px solid #ccc;
    cursor: crosshair;
}

.tool-btn {
    padding: 5px 10px;
    border: none;
    border-radius: 4px;
    background: #4CAF50;
    color: white;
    cursor: pointer;
}

.tool-btn:hover {
    background: #45a049;
}

.color-picker input {
    width: 40px;
    height: 40px;
    padding: 0;
    border: none;
    border-radius: 4px;
    cursor: pointer;
}

.brush-size input {
    width: 100px;
}
</style>