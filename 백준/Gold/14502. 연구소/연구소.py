from collections import deque

n, m = map(int,input().split())
li = []
virus = []
empty = []
res = float('inf')
wall = []
ans = 3

for i in range(n):
    tmp = list(map(int,input().split()))
    li.append(tmp)
    for j in range(m):
        if tmp[j] == 2:
            virus.append([i,j])
        elif tmp[j] == 0:
            empty.append([i,j])
        else:
            ans += 1

def check(wall):
    
    for x,y in wall:
        li[x][y] = 1
    res = bfs()
    
    return res

def bfs():
    visited = [[False]*m for _ in range(n)]
    queue = deque(virus)
    cnt = 0
    direct = [[0,1],[0,-1],[1,0],[-1,0]]
    visited[virus[0][0]][virus[0][1]] = True
    
    while queue:
        x, y = queue.popleft()
        cnt += 1
        for nx, ny in direct:
            dx = x + nx
            dy = y + ny
            if 0 <= dx < n and 0 <= dy < m and not visited[dx][dy] and li[dx][dy] == 0:
                queue.append([dx,dy])
                visited[dx][dy] = True
                
    return cnt
                

def dfs(start):
    global res
    if len(wall) == 3: # 벽 최대 3개
        res = min(res,check(wall))
        #print(wall,res)
        return
    
    for i in range(start,len(empty)):
        x, y = empty[i]
        wall.append([x,y]) # 벽추가
        dfs(i+1)
        li[x][y] = 0
        wall.pop()

dfs(0)
print(n*m - ans - res)