using System;
namespace ChaoticCipher
{
	public class Ball : NonLinearSystem
	{
		/*
		 * See http://en.wikipedia.org/wiki/Bouncing_ball_dynamics
		 * Implemented in 1 dimension
		 * 
		 * b = ball
		 * t = table
		 * x = coordinate
		 * v = velocity
		 * g = gravity
		 * h = max height in metres
		 */
		long xb, xt, vb, vt, g, h=(long)Math.Pow(2, 55), metre;
		
		//sinusoidal amplitude and Hooke's law constant for table
		ushort A;
		decimal hooke = 0.1m;
		
		public Ball ()
		{
			metre = long.MaxValue / h;
			g = (long) Math.Floor(9.81 * metre);
			
			A = (ushort) (metre * 2048);
			
			//table is at a peak -> static
			xt = A;
			vt = 0;
			
			xb = A * 4;
			vb = 0;
		}
		
		public override void Iterate(ushort nudge)
		{
			xt += nudge - 127; //nudge is (ushort) byte
			BoundTable();
			vt -= (long) (hooke * xt);
			xt += vt;
			
			vb -= g;
			long space = xb < 0 ? long.MaxValue : long.MaxValue - xb;

			if(space < vb){ //hit the roof
				xb = long.MaxValue - (vb-space);
				vb = -vb; //no dissapation and roof is infinitely heavy
			}
			else {
				xb += vb;
				if(xb < xt){ //hit the table
					xb += (xt - xb);
					vb = -vb + vt; //no dissapation and table is infinitely heavy
				}
			}
		}
		
		public override ulong RandomSource {
			get { return (ulong) Math.Abs(xb); }
		}
		
		private void BoundTable(){
			xt = (long) Math.Max(-A, Math.Min(A, xt));
		}
	}
}

