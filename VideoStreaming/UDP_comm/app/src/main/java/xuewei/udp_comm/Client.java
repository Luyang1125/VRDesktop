package xuewei.udp_comm;

import android.app.Activity;
import android.app.ActivityManager;
import android.app.Service;
import android.content.Context;
import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.Point;
import android.os.IBinder;
import android.support.annotation.Nullable;
import android.util.DisplayMetrics;
import android.util.Log;
import android.view.Display;
import android.view.WindowManager;
import android.widget.ImageView;

import java.io.IOException;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.util.Arrays;


/**
 * Created by Avril on 2/4/17.
 */

public class Client implements Runnable {

    public static final String SERVER_IP = "192.168.42.21";
    public static final int SERVER_PORT = 5554;
    public static final int LOCAL_PORT = 5554;
    private DatagramSocket socket;
    private long sendTime = 0;
    private InetAddress serverAddr;


    public Client() {
        try {
            serverAddr = InetAddress.getByName(SERVER_IP);
            socket = new DatagramSocket(LOCAL_PORT);
        } catch (Exception e) {
            Log.e("UDP", "Error in creating socket", e);
        }

        new Thread(this).start();
    }



    public void sendRequest(int x, int y) {
        try {
            String point = String.valueOf(x) + ", " + String.valueOf(y);
            byte[] buf = (point).getBytes();
            DatagramPacket packet = new DatagramPacket(buf, buf.length, serverAddr, SERVER_PORT);
            Log.d("UDP", "C: Sending: '" + new String(buf) + "'");
            sendTime = System.nanoTime();
            socket.send(packet);
            Log.d("UDP", "C: Sent.");
        } catch (Exception e) {
            Log.e("UDP", "Error in sending packet", e);
        }
    }

    @Override
    public void run() {
        byte[] receivedata;
        while (true) {
            try {
                //receive real time pics
                receivedata = new byte[64 * 1024];
                DatagramPacket packet = new DatagramPacket(receivedata, receivedata.length);
                Log.d("UDP", "C: Receiving: pic");
                socket.receive(packet);
                Log.d("UDP", String.format("packet size: %d", packet.getLength()));

                //count time
                Log.d("UDP", String.format("time diff: %dns", System.nanoTime() - sendTime));
                Log.d("UDP", "C: Received: pic.");

//                final Bitmap new_img = BitmapFactory.decodeByteArray(buff, 0, packet.getLength());
//                if(new_img == null)
//                    Log.d("UDP", "bitmap == null");
//                else{
//                    MainActivity.image.post(new Runnable() {
//                        @Override
//                        public void run() {
//                            MainActivity.image.setImageBitmap(new_img);
//                        }
//                    });
//                }

                //send feedback
                byte[] feedback = ("feedback").getBytes();
                packet = new DatagramPacket(feedback, feedback.length, serverAddr, SERVER_PORT);
                socket.send(packet);
                Log.d("UDP", "C: Sent: feedback--------------------");
            } catch (Exception e) {
                Log.e("UDP", "Error in receiving packet", e);
            }
        }

//        //client done
//        Log.d("UDP", "C: Done.");
    }
}
