using UnityEngine;
using System.Collections;
using System.Linq;
//by Pepijn Willekens
// https://github.com/peperbol
// https://twitter.com/PepijnWillekens

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour {
	public bool isBounded = false;
	[ConditionalHide("isBounded")]
	public Rect mapBounds;

	public float vecticalSize;

	private Camera cam;
	[Range(0, 1)]
	public float followUpSpeed;
	public Transform[] targetsToFollow;

	[Range(0, 1)]
	public float horizontalWanderPercent, verticalWanderPercent;
	[Range(-0.5f, 0.5f)]
	public float horizontalCenterPercent, verticalCenterPercent;

	private Rect PosToCamBounds(Vector2 pos) {
		return new Rect(pos - CamSize / 2, CamSize);
	}

	private Rect PosToMinCamBounds(Vector2 pos) {
		return new Rect(pos - MinCamSize / 2, MinCamSize);
	}

	private bool ShouldCameraZoom {
		get {
			if (targetsToFollow.Length <= 1) return false;
			Rect tr = RectContainingAllTargetsToFollow;
			Rect mr = MinTargetBounds;
			return tr.height > mr.height || tr.width > mr.width;
        }
	}
	private Rect RectContainingAllTargetsToFollow {
		get {
			return TargetsToFollowPositions.Select(e => new Rect(e, Vector2.zero)).Aggregate((p, n) => ExpandRectToContain(p, n));
		}
	}
	private Vector2[] TargetsToFollowPositions {
		get { return targetsToFollow.Select(e => V2(e.position)).ToArray(); }
	}

	Vector2 AverageTargetToFollow {
		get {
			return TargetsToFollowPositions.Aggregate((p, n) => p + n) / targetsToFollow.Length;
		}
	}

	private bool IsRectInRect(Rect inner, Rect outer) {
		bool toReturn = true;
		toReturn &= inner.yMax <= outer.yMax;
		toReturn &= inner.xMax <= outer.xMax;
		toReturn &= inner.yMin >= outer.yMin;
		toReturn &= inner.xMin >= outer.xMin;
		return toReturn;
	}
	private Rect ClosestContaintingRect(Rect target, Rect container) {

		if (target.yMax > container.yMax) target.y -= target.yMax - container.yMax;
		if (target.xMax > container.xMax) target.x -= target.xMax - container.xMax;
		if (target.yMin < container.yMin) target.y += container.yMin - target.yMin;
		if (target.xMin < container.xMin) target.x += container.xMin - target.xMin;
		return target;
	}

	private Rect ExpandRectToContain(Rect original, Rect target) {

		if (target.yMax > original.yMax) original.yMax += target.yMax - original.yMax;
		if (target.xMax > original.xMax) original.xMax += target.xMax - original.xMax;
		if (target.yMin < original.yMin) original.yMin -= original.yMin - target.yMin;
		if (target.xMin < original.xMin) original.xMin -= original.xMin - target.xMin;
		return original;
	}

	private Vector2 MinCamSize {
		get { return new Vector2(vecticalSize * 2 * cam.aspect, vecticalSize * 2); }
	}
	private Vector2 CamSize {
		get { return new Vector2(cam.orthographicSize * 2 * cam.aspect, cam.orthographicSize * 2); }
	}
	private Vector2 MyPosition {
		get {
			return V2(transform.position);
		}
		set {
			if (isBounded) {
				Rect r = ClosestContaintingRect(PosToCamBounds(value), mapBounds);
				transform.position = new Vector3(r.center.x, r.center.y, transform.position.z);
			}
			else {
				transform.position = ((Vector3)value).SetZ(transform.position.z);
			}

		}
	}
	private Vector2 V2(Vector3 v3) {
		return new Vector2(v3.x, v3.y);
	}
	private Rect TargetBoundsToCamBounds(Rect targetBounds) {
		Vector2 c = targetBounds.center;
		targetBounds.width /= horizontalWanderPercent;
		targetBounds.height /= verticalWanderPercent;
		targetBounds.center = c ;
		return targetBounds;
	}
	private Rect MinTargetBounds {
		get {
			Rect toReturn = PosToMinCamBounds(MyPosition);
			Vector2 c = MyPosition + new Vector2(toReturn.width * horizontalCenterPercent, toReturn.height * verticalCenterPercent);
			toReturn.width *= horizontalWanderPercent;
			toReturn.height *= verticalWanderPercent;
			toReturn.center = c;
			return toReturn;
		}
	}
	private Rect GrowRectToAspect (Rect rect, float aspect) {
		Vector2 c = rect.center;
        if (rect.height * aspect > rect.width) {
			rect.width = rect.height * aspect;
		}
		else {
			rect.height = rect.width / aspect;
		}
		rect.center = c;
		return rect;
	}

	private Vector2 TargetPos(Vector2 target) {
		return ClosestContaintingRect(new Rect(target, Vector2.zero), MinTargetBounds).position;
	}

	void Move(Vector2 offset) {
		MyPosition += offset;
	}
	// Use this for initialization
	void Start() {
		cam = GetComponent<Camera>();

		SetNewValues(1);

        if (!Application.isEditor)
			Cursor.visible = false;
	}


	// Update is called once per frame
	void Update() {
		SetNewValues(followUpSpeed);
    }
	void SetNewValues(float lerpValue) {


		if (ShouldCameraZoom) {
			Rect targetBounds = TargetBoundsToCamBounds(GrowRectToAspect(RectContainingAllTargetsToFollow, cam.aspect));

			cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetBounds.height / 2, lerpValue);
			Move(Vector2.Lerp(Vector2.zero,  targetBounds.center - MyPosition, lerpValue));
		}
		else {
			cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, vecticalSize, lerpValue);
			Vector2[] targets = TargetsToFollowPositions;
			for (int i = 0; i < targets.Length; i++) {

				Move(Vector2.Lerp(Vector2.zero, targets[i] - TargetPos(targets[i]), lerpValue));
			}

		}
	}
}
