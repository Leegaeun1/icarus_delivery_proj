n, m = map(int,input().split())
li = []
chicken = []
home = []
res = float('inf')
store = []

for i in range(n):
    tmp = list(map(int,input().split()))
    li.append(tmp)
    for j in range(n):
        if tmp[j] == 2:
            chicken.append([i,j])
        elif tmp[j] == 1:
            home.append([i,j])

def dfs(start):
    global res
    if len(store) == m: # 벽 최대 3개
        min_dis = 0
        for x, y in home:
            tmp = float('inf')
            for dx, dy in store:
                tmp = min(tmp, abs(x-dx)+abs(y-dy))
            min_dis += tmp
        #print(store,min_dis)
        res = min(min_dis,res)
        return
    
    for i in range(start,len(chicken)):
        x, y = chicken[i]
        store.append((x,y))
        dfs(i+1)
        store.pop()

dfs(0)
print(res)