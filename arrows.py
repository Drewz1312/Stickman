import tkinter as tk
from PIL import Image, ImageTk
import time

root = tk.Tk()
root.geometry("500x400")

canvas = tk.Canvas(root, bg='white', width=500, height=400)
canvas.pack()

# Load images
walk_img = ImageTk.PhotoImage(Image.open("Animation/TCS.png").resize((100, 100), Image.Resampling.LANCZOS))
jump_img = ImageTk.PhotoImage(Image.open("Animation/Jump.png").resize((100, 100), Image.Resampling.LANCZOS))

x = 250
y = 200
img_id = canvas.create_image(x, y, image=walk_img)

is_jumping = False

def jump(e):
    global is_jumping, y
    if is_jumping:
        return
    
    is_jumping = True
    canvas.itemconfig(img_id, image=jump_img)
    
    # Jump physics
    start_y = y
    peak_y = y - 80
    duration = 0.4
    steps = 20
    step_time = duration / steps
    
    for i in range(steps + 1):
        t = i / steps
        # Parabolic arc: up then down
        if t <= 0.5:
            y_pos = start_y - (start_y - peak_y) * (t * 2)
        else:
            y_pos = peak_y + (start_y - peak_y) * ((t - 0.5) * 2)
        
        canvas.coords(img_id, x, y_pos)
        root.update()
        time.sleep(step_time)
    
    y = start_y
    canvas.coords(img_id, x, y)
    canvas.itemconfig(img_id, image=walk_img)
    is_jumping = False

def move(e):
    global x
    step = 20
    if e.keysym == 'Left':
        x -= step
    elif e.keysym == 'Right':
        x += step
    canvas.coords(img_id, x, y)

root.bind('<Left>', move)
root.bind('<Right>', move)
root.bind('<Up>', jump)
root.mainloop()
