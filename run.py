import subprocess
import os
import sys
from threading import Thread
import re

def run_command(command, cwd, print_output=True):
    """执行命令并可选地打印输出"""
    try:
        # 启动子进程，捕获 stdout 和 stderr
        process = subprocess.Popen(
            command,
            cwd=cwd,
            shell=True,
            stdout=subprocess.PIPE,
            stderr=subprocess.STDOUT,
            text=True,
            bufsize=1,
            universal_newlines=True,
            encoding='utf-8',  # 指定UTF-8编码
            errors='replace'   # 替换无法解码的字符（可选）
        )

        # 实时打印输出的线程函数
        def print_thread():
            pattern = re.compile(r'^[\s]*([\u4e00-\u9fa5]|\[)')
            for line in process.stdout:
                if print_output:
                    prefix = "[Backend] " if "backend" in cwd else ""
                    # 仅处理后端输出
                    if "backend" in cwd:
                        # 过滤：仅保留中文开头或[开头的行（允许前面有空格）
                        if not pattern.match(line):
                            continue  # 不满足条件的行跳过
                    # 打印符合条件的内容
                    print(f"{prefix}{line}", end="")

        # 启动打印线程
        if print_output:
            Thread(target=print_thread, daemon=True).start()

        # 等待进程完成
        process.wait()
        
        # 检查是否有错误
        if process.returncode != 0:
            print(f"命令执行失败 (返回码: {process.returncode}): {command}")
            return False
        return True

    except Exception as e:
        print(f"执行命令时出错: {str(e)}")
        return False

def main():
    # 获取当前脚本所在目录（作为项目根目录）
    project_root = os.path.dirname(os.path.abspath(__file__))
    
    # 定义前后端目录和命令
    frontend_dir = os.path.join(project_root, "frontend")
    backend_dir = os.path.join(project_root, "backend")
    
    frontend_command = "npm run dev"
    backend_command = "dotnet run"

    # 检查目录是否存在
    if not os.path.isdir(frontend_dir):
        print(f"前端目录不存在: {frontend_dir}")
        sys.exit(1)
    
    if not os.path.isdir(backend_dir):
        print(f"后端目录不存在: {backend_dir}")
        sys.exit(1)

    print("=== 启动前端服务 ===")
    frontend_thread = Thread(
        target=run_command,
        args=(frontend_command, frontend_dir, False),  # 前端输出不打印
        daemon=True
    )
    frontend_thread.start()

    print("\n=== 启动后端服务 ===")
    # 后端输出实时打印
    backend_success = run_command(backend_command, backend_dir, True)

    # 如果后端服务退出，终止前端服务
    print("\n=== 后端服务已退出 ===")
    sys.exit(0 if backend_success else 1)

if __name__ == "__main__":
    main()