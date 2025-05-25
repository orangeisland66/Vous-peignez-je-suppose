<template>
  <div class="app">
    <nav class="navbar" :class="{ show: showNavbar }">
      <div class="container">
        <router-link to="/lobby" class="logo">你画我猜</router-link>
        <div class="nav-links">
          <router-link to="/lobby" class="nav-link">大厅</router-link>
          <router-link to="/room/create" class="nav-link">创建房间</router-link>
          <router-link to="/profile" class="nav-link">个人中心</router-link>
        </div>
      </div>
    </nav>

    <main class="main-content">
      <div class="container main-container">
        <router-view />
      </div>
    </main>
  </div>
</template>

<script>
export default {
  name: 'App',
  data() {
    return {
      showNavbar: false
    };
  },
  mounted() {
    document.addEventListener('mousemove', this.handleMouseMove);
  },
  beforeUnmount() {
    document.removeEventListener('mousemove', this.handleMouseMove);
  },
  methods: {
    handleMouseMove(e) {
      this.showNavbar = e.clientY < 72;
    }
  }
};
</script>

<style>
:root {
  --dai-green: #426666;
  --dai-green-light: #587878;
  --dai-green-lighter: #e8f0f0;
  --navbar-bg: rgba(255, 255, 255, 0.6);
  --navbar-blur: blur(10px);
  --text-dark: #2c3e50;
  --container-width: 100%;
  --container-padding: 0 20px;
  --max-content-width: 1200px;
}

* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
  -webkit-overflow-scrolling: touch;
}

body,
html,
#app {
  font-family: "Microsoft YaHei", "PingFang SC", "Helvetica Neue", Arial, sans-serif;
  height: 100%;
  margin: 0;
  padding: 0;
  overflow: hidden;
}

.app {
  height: 100vh;
  width: 100vw;
  display: flex;
  flex-direction: column;
  background-color: var(--dai-green-pale, #f8f8f8);
  overflow: hidden;
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
}

.container {
  width: var(--container-width);
  max-width: var(--max-content-width);
  padding: var(--container-padding);
  margin: 0 auto;
  display: flex;
  justify-content: space-between;
  align-items: center;
  box-sizing: border-box;
}

/* ===== NAVBAR ===== */
.navbar {
  position: fixed;
  top: -80px;
  left: 0;
  width: 100%;
  height: 50px;
  padding: 4px;
  background-color: var(--navbar-bg);
  backdrop-filter: var(--navbar-blur);
  -webkit-backdrop-filter: var(--navbar-blur);
  border-bottom: 1px solid rgba(255, 255, 255, 0.4);
  box-shadow: 0 6px 20px rgba(66, 102, 102, 0.1);
  z-index: 1000;
  transition: top 0.3s ease;
}

.navbar.show {
  top: 0;
}

.logo {
  font-size: 24px;
  font-weight: 600;
  color: var(--dai-green);
  text-decoration: none;
  transition: opacity 0.2s;
}

.logo:hover {
  opacity: 0.7;
}

.nav-links {
  display: flex;
  gap: 20px;
}

.nav-link {
  color: var(--text-dark);
  text-decoration: none;
  font-size: 16px;
  padding: 8px 12px;
  border-radius: 6px;
  transition: background 0.3s, transform 0.2s, color 0.3s;
}

.nav-link:hover {
  background-color: rgba(88, 120, 120, 0.1);
  transform: translateY(-2px);
  color: var(--dai-green);
}

/* ===== MAIN CONTENT ===== */
.main-content {
  flex: 1;
  display: flex;
  width: 100%;
  overflow: hidden;
}

.main-container {
  flex-direction: column;
  width: 100%;
  height: 100%;
  justify-content: flex-start;
  overflow-y: hidden;
  overflow-x: hidden;
  padding-top: 0;
}
</style>
