<template>
  <footer class="app-footer">
    <div class="container">
      <div class="footer-content">
        <div class="footer-info">
          <p class="copyright">
            &copy; {{ currentYear }} DrawGuess. All rights reserved.
          </p>
          <p class="description">
            一个有趣的在线绘画猜词游戏，让您与朋友一起享受绘画和猜词的乐趣。
          </p>
        </div>

        <div class="footer-links">
          <div class="link-group">
            <h4>关于我们</h4>
            <a href="/about">关于游戏</a>
            <a href="/rules">游戏规则</a>
            <a href="/contact">联系我们</a>
          </div>

          <div class="link-group">
            <h4>帮助中心</h4>
            <a href="/faq">常见问题</a>
            <a href="/guide">新手指南</a>
            <a href="/feedback">意见反馈</a>
          </div>

          <div class="link-group">
            <h4>社区</h4>
            <a href="/forum">游戏论坛</a>
            <a href="/discord">Discord</a>
            <a href="/wechat">微信公众号</a>
          </div>
        </div>
      </div>
    </div>
  </footer>
</template>

<script>
export default {
  name: 'AppFooter',
  computed: {
    currentYear() {
      return new Date().getFullYear()
    }
  }
}
</script>

<style scoped>
.app-footer {
  background: white;
  padding: 40px 0;
  margin-top: 60px;
  border-top: 1px solid var(--border-color);
}

.footer-content {
  display: grid;
  grid-template-columns: 1fr 2fr;
  gap: 40px;
}

.footer-info {
  max-width: 400px;
}

.copyright {
  color: var(--text-color);
  margin-bottom: 10px;
}

.description {
  color: var(--text-color-light);
  line-height: 1.6;
}

.footer-links {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 30px;
}

.link-group h4 {
  color: var(--primary-color);
  margin-bottom: 15px;
}

.link-group {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.link-group a {
  color: var(--text-color);
  text-decoration: none;
  transition: color 0.3s ease;
}

.link-group a:hover {
  color: var(--primary-color);
}

@media (max-width: 768px) {
  .footer-content {
    grid-template-columns: 1fr;
  }

  .footer-links {
    grid-template-columns: 1fr;
    gap: 20px;
  }
}
</style>
