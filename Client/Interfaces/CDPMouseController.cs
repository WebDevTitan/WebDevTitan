using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using ChromeDevTools.Protocol.Chrome.Input;
using MasterDevs.ChromeDevTools;
using MasterDevs.ChromeDevTools.Protocol.Chrome.Input;
using Project.Helphers;

namespace Project.Interfaces
{
    public class CDPMouseController
    {
        public static CDPMouseController _instance = null;
        public static IChromeSession session = null;
        private BezierCurve bc = new BezierCurve();
        private static readonly NormalDistribution targetDistribution = new NormalDistribution();
        private static readonly NormalDistribution midpointDistribution = new NormalDistribution();
        private Point cur_point = new Point(0, 0);
        Random random = new Random();
        int mouseSpeed = 20;
        public enum MoveMethod
        {
            SQRT,
            BEZIER
        }
        public static CDPMouseController Instance
        {
            get
            {
                return _instance;
            }
        }
        public CDPMouseController(IChromeSession _session)
        {
            session = _session;
        }
        public static void CreateInstance(IChromeSession _session)
        {
            _instance = new CDPMouseController(_session);
        }
        public void MouseClick(Point point, int clickCnt = 1)
        {
            try
            {
                cur_point = point;
                long button = (long)MouseButton.Left;
                session.SendAsync(new DispatchMouseEventCommand { Type = "mousePressed", Button = "left", ClickCount = clickCnt, Buttons = button, X = point.X, Y = point.Y });
                Thread.Sleep(600);
                session.SendAsync(new DispatchMouseEventCommand { Type = "mouseReleased", Button = "left", ClickCount = clickCnt, Buttons = button, X = point.X, Y = point.Y });
            }
            catch { }
        }

        public bool InputText(string text)
        {
            try
            {
                session.SendAsync(new ImeSetCompositionCommand { Text = text, SelectionStart = 0, SelectionEnd = (long)text.Length });
                Thread.Sleep(800);
            }
            catch { }
            return true;
        }

        public void PressKeyboard(string key = "")
        {
            try
            {
                if (key == "Tab")
                {
                    session.SendAsync(new DispatchKeyEventCommand { Type = "rawKeyDown", Key = "Tab", Code = "Tab", NativeVirtualKeyCode = 9, WindowsVirtualKeyCode = 9 });
                    session.SendAsync(new DispatchKeyEventCommand { Type = "keyUp", Key = "Tab", Code = "Tab", NativeVirtualKeyCode = 9, WindowsVirtualKeyCode = 9 });
                }
                else if (key == "Enter")
                {
                    session.SendAsync(new DispatchKeyEventCommand { Type = "rawKeyDown", NativeVirtualKeyCode = 13, Text = "", UnmodifiedText = "", WindowsVirtualKeyCode = 13 });
                    session.SendAsync(new DispatchKeyEventCommand { Type = "char", Text = "\r", NativeVirtualKeyCode = 13, UnmodifiedText = "\r", WindowsVirtualKeyCode = 13 });
                    session.SendAsync(new DispatchKeyEventCommand { Type = "keyUp", Text = "", UnmodifiedText = "", NativeVirtualKeyCode = 13, WindowsVirtualKeyCode = 13 });
                }


                Thread.Sleep(800);
            }
            catch { }
        }
        public async Task<bool> MouseMovement(Point target, MoveMethod moveMethod = MoveMethod.BEZIER)
        {
            if (moveMethod == MoveMethod.BEZIER)
                await MoveWithBezier(target);
            else
            {
                Point closeToEndPos = new Point(
                                       target.X + Utils.GetRandValue(5, 30, true),
                                       target.Y + Utils.GetRandValue(5, 30, true)
                                     );
                await MoveWithSqrt(target);
            }

            return true;
        }
        public async Task<bool> MoveWithBezier(Point target)
        {
            try
            {
                int pointerAccuracy = 10; //assume 10 pixels is the accuracy of this particular human's point and click

                //calculate an X-Y offset to mimic the accuracy of a human
                int targetX = target.X + Convert.ToInt32(pointerAccuracy * targetDistribution.NextGaussian());
                int targetY = target.Y + Convert.ToInt32(pointerAccuracy * targetDistribution.NextGaussian());
                //declare the original pointer position
                int originalX = cur_point.X;
                int originalY = cur_point.Y;
                //find a mid point between original and target position
                int midPointX = (target.X - targetX) / 2;
                int midPointY = (target.Y - targetY) / 2;
                //Find a co-ordinate normal to the straight line between start and end point, starting at the midpoint and normally distributed
                //This is reduced by a factor of 4 to model the arc of a right handed user.
                int bezierMidPointX = Convert.ToInt32((midPointX / 4) * (midpointDistribution.NextGaussian()));
                int bezierMidPointY = Convert.ToInt32((midPointY / 4) * (midpointDistribution.NextGaussian()));

                BezierCurve bc = new BezierCurve();
                double[] input = new double[] { originalX, originalY, bezierMidPointX, bezierMidPointY, targetX, targetY };

                int numberOfDataPoints = 1000;
                double[] output = new double[numberOfDataPoints];

                //co-ords are couplets of doubles hence the / 2
                bc.Bezier2D(input, numberOfDataPoints / 2, output);
                int pause = 0;
                List<System.Drawing.Point> points = new List<Point>();
                for (int count = 1; count != numberOfDataPoints - 1; count += 2)
                {
                    Point point = new Point((int)output[count + 1], (int)output[count]);
                    points.Add(point);
                    await session.SendAsync(new EmulateTouchFromMouseEventCommand { Type = "mouseMoved", Button = "none", X = point.X, Y = point.Y });
                    if ((count % 10) == 0)
                        pause = 2 + ((count ^ 5) / (count * 2));

                    Thread.Sleep(pause);
                    cur_point = point;
                }
            }
            catch { }
            return true;
        }
        public async Task<bool> MoveWithSqrt(Point ePos)
        {
            try
            {
                int x = ePos.X;
                int y = ePos.Y;
                double randomSpeed = Math.Max((random.Next(mouseSpeed) / 2.0 + mouseSpeed) / 10.0, 0.1);
                await WindMouse((double)cur_point.X, (double)cur_point.Y, (double)x, (double)y, 9.0, 3.0, 10.0 / randomSpeed,
                    15.0 / randomSpeed, 10.0 * randomSpeed, 10.0 * randomSpeed);
            }
            catch { }
            return true;
        }
        public async Task<bool> WindMouse(double xs, double ys, double xe, double ye,
            double gravity, double wind, double minWait, double maxWait,
            double maxStep, double targetArea)
        {

            double dist, windX = 0, windY = 0, veloX = 0, veloY = 0, randomDist, veloMag, step;
            int oldX, oldY, newX = (int)Math.Round(xs), newY = (int)Math.Round(ys);

            double waitDiff = maxWait - minWait;
            double sqrt2 = Math.Sqrt(2.0);
            double sqrt3 = Math.Sqrt(3.0);
            double sqrt5 = Math.Sqrt(5.0);

            dist = Hypot(xe - xs, ye - ys);

            while (dist > 1.0)
            {

                wind = Math.Min(wind, dist);

                if (dist >= targetArea)
                {
                    int w = random.Next((int)Math.Round(wind) * 2 + 1);
                    windX = windX / sqrt3 + (w - wind) / sqrt5;
                    windY = windY / sqrt3 + (w - wind) / sqrt5;
                }
                else
                {
                    windX = windX / sqrt2;
                    windY = windY / sqrt2;
                    if (maxStep < 3)
                        maxStep = random.Next(3) + 3.0;
                    else
                        maxStep = maxStep / sqrt5;
                }

                veloX += windX;
                veloY += windY;
                veloX = veloX + gravity * (xe - xs) / dist;
                veloY = veloY + gravity * (ye - ys) / dist;

                if (Hypot(veloX, veloY) > maxStep)
                {
                    randomDist = maxStep / 2.0 + random.Next((int)Math.Round(maxStep) / 2);
                    veloMag = Hypot(veloX, veloY);
                    veloX = (veloX / veloMag) * randomDist;
                    veloY = (veloY / veloMag) * randomDist;
                }

                oldX = (int)Math.Round(xs);
                oldY = (int)Math.Round(ys);
                xs += veloX;
                ys += veloY;
                dist = Hypot(xe - xs, ye - ys);
                newX = (int)Math.Round(xs);
                newY = (int)Math.Round(ys);

                if (oldX != newX || oldY != newY)
                {
                    cur_point = new Point(newX, newY);
                    await session.SendAsync(new EmulateTouchFromMouseEventCommand { Type = "mouseMoved", Button = "none", X = newX, Y = newY });
                }

                step = Hypot(xs - oldX, ys - oldY);
                int wait = (int)Math.Round(waitDiff * (step / maxStep) + minWait);
                Thread.Sleep(wait);
            }

            int endX = (int)Math.Round(xe);
            int endY = (int)Math.Round(ye);
            if (endX != newX || endY != newY)
            {
                cur_point = new Point(endX, endY);
                await session.SendAsync(new EmulateTouchFromMouseEventCommand { Type = "mouseMoved", Button = "none", X = endX, Y = endY });
            }

            return true;
        }
        public double Hypot(double dx, double dy)
        {
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }

}
