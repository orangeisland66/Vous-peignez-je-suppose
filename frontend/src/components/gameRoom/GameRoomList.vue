<template>
    <div class="game-room-list">
        <div class="controls">
            <h2>Game Rooms</h2>
            <el-button type="primary" @click="createRoom">Create Room</el-button>
            <el-input
                v-model="searchQuery"
                placeholder="Search rooms..."
                class="search-input"
            />
        </div>

        <el-table
            v-loading="loading"
            :data="filteredRooms"
            style="width: 100%"
        >
            <el-table-column prop="name" label="Room Name" />
            <el-table-column prop="playerCount" label="Players" width="120">
                <template #default="{ row }">
                    {{ row.currentPlayers }}/{{ row.maxPlayers }}
                </template>
            </el-table-column>
            <el-table-column prop="status" label="Status" width="120" />
            <el-table-column label="Actions" width="120">
                <template #default="{ row }">
                    <el-button
                        :disabled="row.currentPlayers >= row.maxPlayers"
                        type="primary"
                        size="small"
                        @click="joinRoom(row.id)"
                    >
                        Join
                    </el-button>
                </template>
            </el-table-column>
        </el-table>

        <div class="pagination">
            <el-pagination
                :current-page="currentPage"
                :page-size="pageSize"
                :total="totalRooms"
                @current-change="handlePageChange"
                layout="prev, pager, next"
            />
        </div>
    </div>
</template>

<script>
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useStore } from 'vuex'

export default {
    name: 'GameRoomList',
    setup() {
        const router = useRouter()
        const store = useStore()
        const loading = ref(false)
        const searchQuery = ref('')
        const currentPage = ref(1)
        const pageSize = ref(10)
        const totalRooms = ref(0)

        const rooms = ref([])

        const filteredRooms = computed(() => {
            return rooms.value.filter(room => 
                room.name.toLowerCase().includes(searchQuery.value.toLowerCase())
            )
        })

        const fetchRooms = async () => {
            loading.value = true
            try {
                const response = await store.dispatch('gameRoom/fetchRooms', {
                    page: currentPage.value,
                    pageSize: pageSize.value
                })
                rooms.value = response.rooms
                totalRooms.value = response.total
            } catch (error) {
                console.error('Failed to fetch rooms:', error)
            } finally {
                loading.value = false
            }
        }

        const createRoom = () => {
            router.push('/room/create')
        }

        const joinRoom = async (roomId) => {
            try {
                await store.dispatch('gameRoom/joinRoom', roomId)
                router.push(`/room/${roomId}`)
            } catch (error) {
                console.error('Failed to join room:', error)
            }
        }

        const handlePageChange = (page) => {
            currentPage.value = page
            fetchRooms()
        }

        onMounted(() => {
            fetchRooms()
        })

        return {
            loading,
            searchQuery,
            currentPage,
            pageSize,
            totalRooms,
            filteredRooms,
            createRoom,
            joinRoom,
            handlePageChange
        }
    }
}
</script>

<style scoped>
.game-room-list {
    padding: 20px;
}

.controls {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 20px;
}

.search-input {
    width: 200px;
}

.pagination {
    margin-top: 20px;
    display: flex;
    justify-content: center;
}
</style>