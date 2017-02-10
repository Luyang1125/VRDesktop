package xuewei.udp_comm;

import android.content.Context;
import android.graphics.Point;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.util.DisplayMetrics;
import android.util.Log;
import android.view.MotionEvent;
import android.view.View;
import android.view.WindowManager;
import android.widget.Button;
import android.widget.ImageView;

import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;

public class MainActivity extends AppCompatActivity {

    private Client client = new Client();
    public static ImageView image;
    private static Button btn_connect;
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        //get screen width and height pixels, this method gets the same result with getMetrics() method.
        WindowManager wm = (WindowManager) getSystemService(Context.WINDOW_SERVICE);
        int w = wm.getDefaultDisplay().getWidth();
        int h = wm.getDefaultDisplay().getHeight();
        Log.d("DISPLAY", "wm: width: " + w + "  height: " + h);

        //Get x,y coordinate of a touch point, which I think integer is better to represent them.
        //Each time user touch a file (at x,y on the screen), server will send corresponding content.
        image = (ImageView) findViewById(R.id.imagetest);
        image.setOnTouchListener(new View.OnTouchListener() {
            @Override
            public boolean onTouch(View v, MotionEvent event) {
                final int x = (int) event.getX();
                final int y = (int) event.getY();
                Log.d("DISPLAY", "touching point: (" + x + ", " + y + ")");
                new Thread(new Runnable() {
                    @Override
                    public void run() {
                        client.sendRequest(x, y);
                    }
                }).start();
                return false;
            }
        });

//        btn_connect = (Button) findViewById(R.id.btn_connect);
//        btn_connect.setOnClickListener(new View.OnClickListener() {
//            @Override
//            public void onClick(View v) {
//
//                new Thread(new Runnable() {
//                    @Override
//                    public void run() {
//                        client.sendRequest();
//                    }
//                }).start();
//            }
//        });
    }
}
